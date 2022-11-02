// MIT License.

using System.CodeDom.Compiler;
using System.Collections.Immutable;
using System.Globalization;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.SystemWebAdapters.Compiler;
using Microsoft.AspNetCore.SystemWebAdapters.Compiler.Symbols;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.SystemWebAdapters.UI.RuntimeCompilation;

internal sealed class RoslynPageCompiler : IPageCompiler
{
    private static readonly Memory<byte> NotTypeFoundMessage = Encoding.UTF8.GetBytes("Could not find class in generated assembly");

    private readonly bool _isDebug;
    private readonly ILogger<RoslynPageCompiler> _logger;
    private readonly ILoggerFactory _factory;
    private readonly IOptions<PageCompilationOptions> _options;

    private bool _isCompiling;

    public RoslynPageCompiler(ILoggerFactory factory, IHostEnvironment env, IOptions<PageCompilationOptions> options)
    {
        _isDebug = env.IsDevelopment();
        _logger = factory.CreateLogger<RoslynPageCompiler>();
        _factory = factory;
        _options = options;
    }

    public async Task<ICompiledPage> CompilePageAsync(IFileProvider files, string path, CancellationToken token)
    {
        if (_isCompiling)
        {
            throw new InvalidOperationException("Compilation cannot be parallel");
        }

        _isCompiling = true;

        try
        {
            return await CompilePageInternalAsync(files, path, token).ConfigureAwait(false);
        }
        finally
        {
            _isCompiling = false;
        }
    }

    public async Task<ICompiledPage> CompilePageInternalAsync(IFileProvider files, string path, CancellationToken token)
    {
        _logger.LogTrace("Compiling {Path}", path);

        var references = GetMetadataReferences();

        var directory = Path.GetDirectoryName(path)!;

        var writingResult = await GetSourceAsync(files, path, token).ConfigureAwait(false);
        var dependentFiles = writingResult
            .UserFiles
            .Select(f => f.Path.Trim('/')).ToArray();

        if (writingResult.ErrorMessage is { } errorMessage)
        {
            return new CompiledPage(writingResult.File, dependentFiles) { Error = Encoding.UTF8.GetBytes(errorMessage) };
        }

        if (writingResult is { Errors.IsDefault: false, Errors.IsEmpty: false })
        {
            return new CompiledPage(writingResult.File, dependentFiles) { Error = JsonSerializer.SerializeToUtf8Bytes(writingResult.Errors) };
        }

        var trees = writingResult.SourceFiles
            .Concat(writingResult.GeneratedSourceFiles)
            .Select(result => CSharpSyntaxTree.ParseText(result.Text, cancellationToken: token)
                .WithFilePath(result.Path));

        var optimization = _isDebug ? OptimizationLevel.Debug : OptimizationLevel.Release;

        var compilation = CSharpCompilation.Create($"WebForms.{writingResult.File.ClassName}",
            options: new CSharpCompilationOptions(
                outputKind: OutputKind.DynamicallyLinkedLibrary,
                optimizationLevel: optimization),
            syntaxTrees: trees,
            references: references);

        using var peStream = new MemoryStream();
        using var pdbStream = new MemoryStream();

        var embeddedTexts = writingResult.AllFiles
            .Select(result => EmbeddedText.FromSource(result.Path, result.Text));

        var result = compilation.Emit(
            embeddedTexts: embeddedTexts,
            peStream: peStream,
            pdbStream: pdbStream,
            cancellationToken: token);

        if (!result.Success)
        {
            _logger.LogWarning("{ErrorCount} error(s) found compiling {Route}", result.Diagnostics.Length, writingResult.File.UrlPath);

            var error = result.Diagnostics
                .Select(d => new
                {
                    d.Id,
                    Message = d.GetMessage(CultureInfo.CurrentCulture),
                    Severity = d.Severity,
                    Location = d.Location.ToString(),
                })
                .OrderByDescending(d => d.Severity);

            var message = JsonSerializer.SerializeToUtf8Bytes(error, new JsonSerializerOptions() { Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() } });

            return new CompiledPage(writingResult.File, dependentFiles) { Error = message };
        }

        pdbStream.Position = 0;
        peStream.Position = 0;

        var context = new PageAssemblyLoadContext(writingResult.File.UrlPath, _factory.CreateLogger<PageAssemblyLoadContext>());
        var assembly = context.LoadFromStream(peStream, pdbStream);
        if (assembly.GetType(writingResult.File.ClassName) is Type type)
        {
            return new CompiledPage(writingResult.File, dependentFiles) { Type = type };
        }

        return new CompiledPage(writingResult.File, dependentFiles) { Error = NotTypeFoundMessage };
    }

    private readonly Dictionary<Assembly, MetadataReference> _references = new();

    private IEnumerable<MetadataReference> GetMetadataReferences()
    {
        var references = new List<MetadataReference>();

        foreach (var assembly in AssemblyLoadContext.Default.Assemblies.Concat(_options.Value.Assemblies))
        {
            if (!assembly.IsDynamic)
            {
                if (!_references.TryGetValue(assembly, out var metadata))
                {
                    metadata = MetadataReference.CreateFromFile(assembly.Location);
                    _references.Add(assembly, metadata);
                }

                references.Add(metadata);
            }
        }

        return references;
    }

    private async Task<WritingResult> GetSourceAsync(IFileProvider files, string filePath, CancellationToken token)
    {
        var paths = new Queue<string>();
        paths.Enqueue(filePath);

        var sourceFiles = new List<(SourceText, string)>();
        var generatedFiles = new List<(SourceText, string)>();
        var nonSourceFiles = new List<(SourceText, string)>();
        var pagePath = new PagePath(filePath);

        while (paths.Count > 0)
        {
            var path = paths.Dequeue();

            using (var stream = new MemoryStream())
            {
                var file = files.GetFileInfo(path);
                var dir = Path.GetDirectoryName(path);
                var contents = await RetryOpenFileAsync(file, token).ConfigureAwait(false);

                contents = contents.Trim();

                using (var streamWriter = new StreamWriter(stream, leaveOpen: true))
                {
                    using var writer = new IndentedTextWriter(streamWriter);

                    var details = AspNetCompiler.ParsePage(path, contents, _options.Value.Info);

                    if (!details.Errors.IsDefaultOrEmpty)
                    {
                        return new WritingResult(pagePath) { Errors = details.Errors };
                    }

                    var cs = new CSharpPageWriter(writer, details);

                    cs.Write();

                    if (!details.Errors.IsDefaultOrEmpty)
                    {
                        return new WritingResult(pagePath) { Errors = details.Errors };
                    }

                    foreach (var additional in details.AdditionalFiles)
                    {
                        var fullAdditional = dir is null ? additional : Path.Combine(dir, additional);

                        if (IsSourceFile(fullAdditional))
                        {
                            var additionalSource = await RetryOpenFileAsync(files.GetFileInfo(fullAdditional), token).ConfigureAwait(false);
                            sourceFiles.Add((SourceText.From(additionalSource, Encoding.UTF8), fullAdditional));
                        }
                        else
                        {
                            paths.Enqueue(fullAdditional);
                        }
                    }
                }

                nonSourceFiles.Add((SourceText.From(contents, Encoding.UTF8), path));

                var bytes = stream.ToArray();
                generatedFiles.Add((SourceText.From(bytes, bytes.Length, Encoding.UTF8, canBeEmbedded: true), $"{path}.g.cs"));
            }
        }

        return new WritingResult(pagePath)
        {
            UserFiles = nonSourceFiles,
            SourceFiles = sourceFiles,
            GeneratedSourceFiles = generatedFiles,
        };
    }

    private static bool IsSourceFile(string path) => path.EndsWith(".cs") || path.EndsWith(".vb");

    private sealed record WritingResult(PagePath File)
    {
        public string? ErrorMessage { get; init; }

        public IReadOnlyCollection<(SourceText Text, string Path)> UserFiles { get; init; } = Array.Empty<(SourceText Text, string Path)>();

        public IReadOnlyCollection<(SourceText Text, string Path)> SourceFiles { get; init; } = Array.Empty<(SourceText, string)>();

        public IReadOnlyCollection<(SourceText Text, string Path)> GeneratedSourceFiles { get; init; } = Array.Empty<(SourceText, string)>();

        public ImmutableArray<string> Errors { get; init; }

        public IEnumerable<(SourceText Text, string Path)> AllFiles => SourceFiles.Concat(UserFiles).Concat(GeneratedSourceFiles);
    }

    private async Task<string> RetryOpenFileAsync(IFileInfo file, CancellationToken token, int retryCount = 5)
    {
        var count = 0;

        while (count < retryCount)
        {
            token.ThrowIfCancellationRequested();
            count++;

            try
            {
                return await GetContentsAsync(file).ConfigureAwait(false);
            }
            catch (IOException) when (count < retryCount)
            {
                _logger.LogWarning("Error accessing {File}. Retrying in 100ms", file.PhysicalPath ?? file.Name);
                await Task.Delay(TimeSpan.FromMilliseconds(100), token).ConfigureAwait(false);
            }
        }

        throw new InvalidOperationException("Could not open file");
    }

    private static async Task<string> GetContentsAsync(IFileInfo file)
    {
        using var stream = file.CreateReadStream();
        using var reader = new StreamReader(stream);

        return await reader.ReadToEndAsync().ConfigureAwait(false);
    }
}
