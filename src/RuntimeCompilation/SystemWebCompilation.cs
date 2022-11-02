// MIT License.

using System.CodeDom.Compiler;
using System.Globalization;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Text.Json;
using System.Web.Compilation;
using System.Web.UI;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.SystemWebAdapters.UI.RuntimeCompilation;

internal sealed class SystemWebCompilation : IPageCompiler
{
    private static readonly Memory<byte> NotTypeFoundMessage = Encoding.UTF8.GetBytes("Could not find class in generated assembly");

    private readonly Dictionary<Assembly, MetadataReference> _references = new();
    private readonly ILoggerFactory _factory;
    private readonly IOptions<PageCompilationOptions> _options;
    private readonly ILogger<SystemWebCompilation> _logger;

    public SystemWebCompilation(ILoggerFactory factory, IOptions<PageCompilationOptions> options, ILogger<SystemWebCompilation> logger)
    {
        _factory = factory;
        _options = options;
        _logger = logger;
    }

    public Task<ICompiledPage> CompilePageAsync(IFileProvider files, string path, CancellationToken token)
        => Task.FromResult(CompilePage(path, token));

    private ICompiledPage CompilePage(string path, CancellationToken token)
    {
        var parser = new PageParser();
        parser.AddAssemblyDependency("SystemWebUISample", true);
        parser.Parse(Array.Empty<string>(), path);

        var generator = new PageCodeDomTreeGenerator(parser);
        var provider = CodeDomProvider.CreateProvider("CSharp");
        var cu = generator.GetCodeDomTree(provider, new System.Web.StringResourceBuilder(), path);

        var writer = new StringWriter();
        provider.GenerateCodeFromCompileUnit(cu, writer, new());
        var source = writer.ToString();

        var trees = new[] { CSharpSyntaxTree.ParseText(source, cancellationToken: token) };
        var references = GetMetadataReferences();

        var typeName = generator.GetInstantiatableFullTypeName();
        var compilation = CSharpCompilation.Create($"WebForms.{typeName}",
            options: new CSharpCompilationOptions(
                outputKind: OutputKind.DynamicallyLinkedLibrary,
                optimizationLevel: OptimizationLevel.Debug),
            syntaxTrees: trees,
            references: references);

        using var peStream = new MemoryStream();
        using var pdbStream = new MemoryStream();

        var result = compilation.Emit(
            peStream: peStream,
            pdbStream: pdbStream,
            cancellationToken: token);

        if (!result.Success)
        {
            _logger.LogWarning("{ErrorCount} error(s) found compiling {Route}", result.Diagnostics.Length, path);

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

            return new CompiledPage(new(path), Array.Empty<string>()) { Error = message };
        }

        pdbStream.Position = 0;
        peStream.Position = 0;

        var context = new PageAssemblyLoadContext(path, _factory.CreateLogger<PageAssemblyLoadContext>());
        var assembly = context.LoadFromStream(peStream, pdbStream);
        if (assembly.GetType(typeName) is Type type)
        {
            return new CompiledPage(new(path), Array.Empty<string>()) { Type = type };
        }

        return new CompiledPage(new(path), Array.Empty<string>()) { Error = NotTypeFoundMessage };
    }

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
}
