// MIT License.

using System.Globalization;
using System.Reflection;
using System.Runtime.Loader;
using System.Web.UI;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace WebForms.Compiler.Dynamic;

internal sealed class DynamicSystemWebCompilation : SystemWebCompilation
{
    private readonly Dictionary<Assembly, MetadataReference> _references = new();
    private readonly IOptions<PageCompilationOptions> _options;
    private readonly ILoggerFactory _factory;
    private readonly ILogger _logger;

    public DynamicSystemWebCompilation(ILoggerFactory factory, IOptions<PageCompilationOptions> options, IOptions<PagesSection> pagesSection, IOptions<CompilationSection> compilationSection)
        : base(factory, pagesSection, compilationSection)
    {
        _logger = factory.CreateLogger<DynamicSystemWebCompilation>();
        _options = options;
        _factory = factory;
    }

    protected override ICompiledPage CreateCompiledPage(
      Compilation compilation,
      string route,
      string typeName,
      IEnumerable<SyntaxTree> trees,
      IEnumerable<MetadataReference> references,
      IEnumerable<EmbeddedText> embedded,
      CancellationToken token)
    {
        using var peStream = new MemoryStream();
        using var pdbStream = new MemoryStream();

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

            return new CompiledPage(new(route), Array.Empty<string>()) { Exception = new RoslynCompilationException(errors) };
        }

        pdbStream.Position = 0;
        peStream.Position = 0;

        var context = new PageAssemblyLoadContext(route, _factory.CreateLogger<PageAssemblyLoadContext>());
        var assembly = context.LoadFromStream(peStream, pdbStream);
        if (assembly.GetType(typeName) is Type type)
        {
            return new CompiledPage(new(route), embedded.Select(t => t.FilePath).ToArray()) { Type = type };
        }

        return new CompiledPage(new(route), Array.Empty<string>()) { Exception = new InvalidOperationException("No type found") };
    }

    protected override IEnumerable<MetadataReference> GetMetadataReferences()
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
}
