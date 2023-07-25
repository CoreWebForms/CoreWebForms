// MIT License.

using System.Globalization;
using System.Text.Json;
using System.Web.UI;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace WebForms.Compiler.Dynamic;

internal sealed class PersistentSystemWebCompilation : SystemWebCompilation, IWebFormsCompiler
{
    private readonly IOptions<PersistentCompilationOptions> _options;
    private readonly IOptions<PageCompilationOptions> _pageOptions;
    private readonly ILogger _logger;

    // TODO: don't have this as a field
    private readonly List<PageDetails> _pages = new();

    public PersistentSystemWebCompilation(
        IOptions<PersistentCompilationOptions> options,
        IOptions<PageCompilationOptions> pageOptions,
        IOptions<PagesSection> pagesSection,
        IOptions<CompilationSection> compilationSection,
        ILoggerFactory factory)
        : base(factory, pagesSection, compilationSection)
    {
        _logger = factory.CreateLogger<PersistentSystemWebCompilation>();
        _options = options;
        _pageOptions = pageOptions;
    }

    protected override ICompiledPage CreateCompiledPage(Compilation compilation, string route, string typeName, IEnumerable<SyntaxTree> trees, IEnumerable<MetadataReference> references, IEnumerable<EmbeddedText> embedded, CancellationToken token)
    {
        // TODO: Handle output better
        using var peStream = File.OpenWrite(Path.Combine(_options.Value.TargetDirectory, $"WebForms.{typeName}.dll"));
        using var pdbStream = File.OpenWrite(Path.Combine(_options.Value.TargetDirectory, $"WebForms.{typeName}.pdb"));

        peStream.SetLength(0);
        pdbStream.SetLength(0);

        var result = compilation.Emit(
            embeddedTexts: embedded,
            peStream: peStream,
            pdbStream: pdbStream,
            cancellationToken: token);

        if (!result.Success)
        {
            _logger.LogWarning("{ErrorCount} error(s) found compiling {Route}", result.Diagnostics.Length, route);

            var errors = result.Diagnostics
                .OrderByDescending(d => d.Severity)
                .Select(d => new RoslynError()
                {
                    Id = d.Id,
                    Message = d.GetMessage(CultureInfo.CurrentCulture),
                    Severity = d.Severity.ToString(),
                    Location = d.Location.ToString(),
                })
                .ToList();

            var errorResult = JsonSerializer.Serialize(errors);

            File.WriteAllText(Path.Combine(_options.Value.TargetDirectory, $"{typeName}.errors.json"), errorResult);

            return new CompiledPage(new(route), Array.Empty<string>()) { Exception = new RoslynCompilationException(errors) };
        }

        _pages.Add(new(route, typeName, $"WebForms.{typeName}"));
        return new CompiledPage(new(route), embedded.Select(t => t.FilePath).ToArray());
    }

    protected override IEnumerable<MetadataReference> GetMetadataReferences()
    {
        foreach (var assembly in _pageOptions.Value.Assemblies)
        {
            if (!assembly.IsDynamic)
            {
                yield return MetadataReference.CreateFromFile(assembly.Location);
            }
        }

        foreach (var r in _options.Value.MetadataReferences)
        {
            yield return r;
        }

        foreach (var r in _options.Value.References)
        {
            yield return MetadataReference.CreateFromFile(r);
        }
    }

    async Task IWebFormsCompiler.CompilePagesAsync(CancellationToken token)
    {
        Environment.CurrentDirectory = _options.Value.InputDirectory;

        var files = _pageOptions.Value.Files!;

        foreach (var file in files!.GetFiles())
        {
            if (file.FullPath.EndsWith(".aspx", StringComparison.OrdinalIgnoreCase))
            {
                await CompilePageAsync(files, file.FullPath, token).ConfigureAwait(false);
            }
        }

        var dataPath = Path.Combine(_options.Value.TargetDirectory, "webforms.pages.json");
        var data = JsonSerializer.Serialize(_pages);
        File.WriteAllText(dataPath, data);
    }

    private sealed record PageDetails(string Path, string Type, string Assembly);
}
