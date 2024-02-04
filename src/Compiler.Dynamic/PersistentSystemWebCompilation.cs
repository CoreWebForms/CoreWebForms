// MIT License.

using System.Collections.Immutable;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Web.UI;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using static WebForms.Compiler.Dynamic.PersistentSystemWebCompilation;

namespace WebForms.Compiler.Dynamic;

internal sealed class PersistentSystemWebCompilation : SystemWebCompilation<PersistedCompiledPage>, IWebFormsCompiler
{
    private readonly IOptions<PersistentCompilationOptions> _options;
    private readonly IOptions<PageCompilationOptions> _pageOptions;
    private readonly ILogger _logger;

    public PersistentSystemWebCompilation(
        IHostEnvironment env,
        IOptions<PersistentCompilationOptions> options,
        IOptions<PageCompilationOptions> pageOptions,
        IOptions<PagesSection> pagesSection,
        IOptions<CompilationSection> compilationSection,
        ILoggerFactory factory)
        : base(env, factory, pageOptions, pagesSection, compilationSection)
    {
        _logger = factory.CreateLogger<PersistentSystemWebCompilation>();
        _options = options;
        _pageOptions = pageOptions;
    }

    protected override PersistedCompiledPage CreateCompiledPage(
        Compilation compilation,
        string route,
        string typeName,
        IEnumerable<SyntaxTree> trees,
        IEnumerable<MetadataReference> references,
        IEnumerable<EmbeddedText> embedded,
        IEnumerable<Assembly> assemblies,
        CancellationToken token)
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
            _logger.LogError("{ErrorCount} error(s) found compiling {Route}", result.Diagnostics.Length, route);

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

            throw new RoslynCompilationException(route, errors);
        }

        return new PersistedCompiledPage(new(route), embedded.Select(t => t.FilePath).ToArray())
        {
            TypeName = typeName,
            Assembly = $"WebForms.{typeName}",
            MetadataReference = compilation.ToMetadataReference(),
        };
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

        try
        {
            foreach (var file in Files!.GetFiles())
            {
                if (file.FullPath.EndsWith(".aspx", StringComparison.OrdinalIgnoreCase))
                {
                    await CompilePageAsync(file.FullPath, token).ConfigureAwait(false);
                }
            }

            var pagesPath = Path.Combine(_options.Value.TargetDirectory, "webforms.pages.json");
            File.WriteAllText(pagesPath, JsonSerializer.Serialize(GetDetails()));
        }
        catch (RoslynCompilationException r)
        {
            foreach (var error in r.Error)
            {
                _logger.LogError("{Id} [{Severity}] {Message} ({Location})", error.Id, error.Severity, error.Message, error.Location);
            }
        }
        catch (Exception e)
        {
            _logger.LogError("Unexpected error: {Message} {Stacktrace}", e.Message, e.StackTrace);
        }
    }

    private IEnumerable<PageDetails> GetDetails()
    {
        foreach (var page in GetPages())
        {
            yield return new PageDetails(page.Path, page.TypeName, page.Assembly);
        }
    }

    protected override PersistedCompiledPage CreateErrorPage(string path, Exception e) => throw e;

    private sealed record PageDetails(string Path, string Type, string Assembly);

    internal sealed class PersistedCompiledPage : CompiledPage
    {
        public PersistedCompiledPage(PagePath path, string[] dependencies)
            : base(path, dependencies)
        {
        }

        public string TypeName { get; init; } = null!;

        public string Assembly { get; init; } = null!;
    }
}
