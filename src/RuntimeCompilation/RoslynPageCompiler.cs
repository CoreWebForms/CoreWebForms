// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CodeDom.Compiler;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Loader;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SystemWebAdapters.UI.PageParser;
using Microsoft.AspNetCore.SystemWebAdapters.UI.PageParser.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.SystemWebAdapters.UI.RuntimeCompilation;

internal sealed class RoslynPageCompiler : IPageCompiler
{
    private static readonly Memory<byte> NotTypeFoundMessage = Encoding.UTF8.GetBytes("Could not find class in generated assembly");

    private readonly bool _isDebug;
    private readonly ILogger<RoslynPageCompiler> _logger;
    private readonly ILoggerFactory _factory;

    public RoslynPageCompiler(ILoggerFactory factory, IHostEnvironment env)
    {
        _isDebug = env.IsDevelopment();
        _logger = factory.CreateLogger<RoslynPageCompiler>();
        _factory = factory;
    }

    public async Task<ICompiledPage> CompilePageAsync(PageFile file, CancellationToken token)
    {
        using var sourceStream = new MemoryStream();
        var writingResult = await WriteSourceAsync(file.Directory, file.File, sourceStream).ConfigureAwait(false);

        if (writingResult.ErrorMessage is { } errorMessage)
        {
            return new CompiledPage(writingResult.Path) { Error = Encoding.UTF8.GetBytes(errorMessage) };
        }

        if (writingResult is { Errors.IsDefault: false, Errors.IsEmpty: false })
        {
            return new CompiledPage(writingResult.Path) { Error = JsonSerializer.SerializeToUtf8Bytes(writingResult.Errors) };
        }

        Debug.Assert(writingResult.ClassName is not null);

        sourceStream.Position = 0;

        var sourceText = SourceText.From(sourceStream, Encoding.UTF8, canBeEmbedded: true);
        var tree = CSharpSyntaxTree.ParseText(sourceText, cancellationToken: token)
            .WithFilePath($"{writingResult.ClassName}.cs");
        var optimization = _isDebug ? OptimizationLevel.Debug : OptimizationLevel.Release;

        var compilation = CSharpCompilation.Create($"WebForms.{writingResult.ClassName}",
            options: new CSharpCompilationOptions(
                outputKind: OutputKind.DynamicallyLinkedLibrary,
                optimizationLevel: optimization),
            syntaxTrees: new[] { tree },
            references: GetMetadataReferences());

        using var peStream = new MemoryStream();
        using var pdbStream = new MemoryStream();

        var embeddedTexts = _isDebug ? new[] { EmbeddedText.FromSource(tree.FilePath, sourceText) } : null;

        var result = compilation.Emit(
            embeddedTexts: embeddedTexts,
            peStream: peStream,
            pdbStream: pdbStream,
            cancellationToken: token);

        if (!result.Success)
        {
            _logger.LogWarning("{ErrorCount} error(s) found compiling {Route}", result.Diagnostics.Length, writingResult.Path);

            var message = JsonSerializer.SerializeToUtf8Bytes(result.Diagnostics);

            return new CompiledPage(writingResult.Path) { Error = message };
        }

        pdbStream.Position = 0;
        peStream.Position = 0;

        var context = new PageAssemblyLoadContext(writingResult.Path, _factory.CreateLogger<PageAssemblyLoadContext>());
        var assembly = context.LoadFromStream(peStream, pdbStream);
        if (assembly.GetType(writingResult.ClassName) is Type type)
        {
            return new CompiledPage(writingResult.Path) { Type = type };
        }

        return new CompiledPage(writingResult.Path) { Error = NotTypeFoundMessage };
    }

    private sealed class CompiledPage : ICompiledPage
    {
        public CompiledPage(PathString path)
        {
            Path = path;
        }

        public Type? Type { get; set; }

        public Memory<byte> Error { get; set; }

        public PathString Path { get; }

        public void Dispose()
        {
            if (Type is not null)
            {
                RemovePage(Type);
            }
        }

        private static void RemovePage(Type type)
        {
            var alc = AssemblyLoadContext.GetLoadContext(type.Assembly);

            if (alc is not PageAssemblyLoadContext)
            {
                throw new InvalidOperationException("Tried to unload something that is not a page");
            }

            alc.Unload();
        }
    }

    private sealed class PageAssemblyLoadContext : AssemblyLoadContext
    {
        private readonly ILogger<PageAssemblyLoadContext> _logger;

        private static long _count;

        private static string GetName(string name)
        {
            var count = Interlocked.Increment(ref _count);

            return $"WebForms:{name}:{count}";
        }

        public PageAssemblyLoadContext(string route, ILogger<PageAssemblyLoadContext> logger)
            : base(GetName(route), isCollectible: true)
        {
            _logger = logger;

            logger.LogInformation("Created assembly for {Path}", Name);

            Unloading += PageAssemblyLoadContext_Unloading;
        }

        private void PageAssemblyLoadContext_Unloading(AssemblyLoadContext obj)
        {
            Unloading -= PageAssemblyLoadContext_Unloading;

            _logger.LogInformation("Unloading assembly load context for {Path}", Name);
        }
    }

    private static IEnumerable<MetadataReference> GetMetadataReferences()
    {
        foreach (var assembly in AssemblyLoadContext.Default.Assemblies)
        {
            if (!assembly.IsDynamic)
            {
                yield return MetadataReference.CreateFromFile(assembly.Location);
            }
        }
    }

    private static async Task<WritingResult> WriteSourceAsync(string directory, IFileInfo file, Stream stream)
    {
        using var streamWriter = new StreamWriter(stream, leaveOpen: true);
        using var writer = new IndentedTextWriter(streamWriter);

        var contents = await GetContentsAsync(file).ConfigureAwait(false);
        var generator = new CSharpPageBuilder(Path.Combine(directory, file.Name), writer, contents);

        if (!generator.Errors.IsDefaultOrEmpty)
        {
            return new WritingResult(generator.Path) { Errors = generator.Errors };
        }

        generator.WriteSource();

        if (!generator.HasDirective)
        {
            return new WritingResult(generator.Path) { ErrorMessage = "File does not have a directive" };
        }

        return new WritingResult(generator.Path) { ClassName = generator.ClassName };
    }

    private sealed record WritingResult(string Path)
    {
        public string? ClassName { get; init; }

        public string? ErrorMessage { get; init; }

        public ImmutableArray<AspxParseError> Errors { get; init; }
    }

    private static async Task<string> GetContentsAsync(IFileInfo file)
    {
        using var stream = file.CreateReadStream();
        using var reader = new StreamReader(stream);

        return await reader.ReadToEndAsync().ConfigureAwait(false);
    }
}
