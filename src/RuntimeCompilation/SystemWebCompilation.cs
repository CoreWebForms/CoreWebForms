// MIT License.

using System.CodeDom.Compiler;
using System.Globalization;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Text.Json;
using System.Web;
using System.Web.Compilation;
using System.Web.UI;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.SystemWebAdapters.UI.RuntimeCompilation;

internal sealed class SystemWebCompilation : IPageCompiler, IDisposable
{
    private readonly Dictionary<Assembly, MetadataReference> _references = new();
    private readonly ILoggerFactory _factory;
    private readonly IOptions<PageCompilationOptions> _options;
    private readonly ILogger<SystemWebCompilation> _logger;
    private readonly ICompiler _csharp;
    private readonly ICompiler _vb;

    public SystemWebCompilation(
        ILoggerFactory factory,
        IOptions<PageCompilationOptions> options,
        IOptions<PagesSection> pagesSection,
        IOptions<CompilationSection> compilationSection,
        ILogger<SystemWebCompilation> logger)
    {
        _factory = factory;
        _options = options;
        _logger = logger;

        _csharp = new CSharpCompiler();
        _vb = new VisualBasicCompiler();

        // TODO: remove these statics and use DI
        MTConfigUtil.Compilation = compilationSection.Value;
        PagesSection.Instance = pagesSection.Value;
    }

    public Task<ICompiledPage> CompilePageAsync(IFileProvider files, string path, CancellationToken token)
    {
        try
        {
            return Task.FromResult(CompilePage(files, path, token));
        }
        catch (HttpParseException ex)
        {
            return Task.FromResult(CompiledPage.FromError(new(path), ex));
        }
    }

    private ICompiledPage CompilePage(IFileProvider files, string path, CancellationToken token)
    {
        var queue = new Queue<string>();
        queue.Enqueue(path);

        var embedded = new List<EmbeddedText>();
        var trees = new List<SyntaxTree>();
        string typeName = null!;

        ICompiler compiler = null!;

        while (queue.Count > 0)
        {
            var currentPath = queue.Dequeue();
            var generator = CreateGenerator(currentPath);

            if (compiler is null)
            {
                compiler = GetProvider(generator.Parser.CompilerType);
            }

            var cu = generator.GetCodeDomTree(compiler.Provider, new System.Web.StringResourceBuilder(), currentPath);

            using var writer = new StringWriter();
            compiler.Provider.GenerateCodeFromCompileUnit(cu, writer, new());
            var source = SourceText.From(writer.ToString(), Encoding.UTF8);

            var filename = $"__{currentPath}.{compiler.Provider.FileExtension}";
            var sourceTree = compiler.ParseText(source, path: filename, cancellationToken: token);

            trees.Add(sourceTree);
            embedded.Add(EmbeddedText.FromSource(filename, source));

            typeName ??= generator.GetInstantiatableFullTypeName();

            foreach (var dep in generator.Parser.SourceDependencies)
            {
                if (dep is string p && files.GetFileInfo(p) is { Exists: true, IsDirectory: false } file)
                {
                    using var stream = file.CreateReadStream();

                    if (p.EndsWith(compiler.Provider.FileExtension, StringComparison.OrdinalIgnoreCase))
                    {
                        var sourceText = SourceText.From(stream, canBeEmbedded: true);
                        trees.Add(compiler.ParseText(sourceText, p, cancellationToken: token));
                        embedded.Add(EmbeddedText.FromSource(p, sourceText));
                    }
                    else
                    {
                        embedded.Add(EmbeddedText.FromStream(p, stream));
                    }
                }
            }

            if (generator.Parser is PageParser { MasterPage: { } master })
            {
                queue.Enqueue(master.Path);
            }
        }

        var references = GetMetadataReferences();

        var compilation = compiler.CreateCompilation(typeName, trees, references);

        using var peStream = new MemoryStream();
        using var pdbStream = new MemoryStream();

        var result = compilation.Emit(
            embeddedTexts: embedded,
            peStream: peStream,
            pdbStream: pdbStream,
            cancellationToken: token);

        if (!result.Success)
        {
            _logger.LogWarning("{ErrorCount} error(s) found compiling {Route}", result.Diagnostics.Length, path);

            var errors = result.Diagnostics
                .Select(d => new
                {
                    d.Id,
                    Message = d.GetMessage(CultureInfo.CurrentCulture),
                    Severity = d.Severity.ToString(),
                    Location = d.Location.ToString(),
                })
                .OrderByDescending(d => d.Severity)
                .ToList();

            return new CompiledPage(new(path), Array.Empty<string>()) { Exception = new RoslynCompilationException(errors) };
        }

        pdbStream.Position = 0;
        peStream.Position = 0;

        var context = new PageAssemblyLoadContext(path, _factory.CreateLogger<PageAssemblyLoadContext>());
        var assembly = context.LoadFromStream(peStream, pdbStream);
        if (assembly.GetType(typeName) is Type type)
        {
            return new CompiledPage(new(path), Array.Empty<string>()) { Type = type };
        }

        return new CompiledPage(new(path), Array.Empty<string>()) { Exception = new InvalidOperationException("No type found") };
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

    private ICompiler GetProvider(CompilerType compiler)
    {
        if (compiler.IsCSharp())
        {
            return _csharp;
        }

        if (compiler.IsVisualBasic())
        {
            return _vb;
        }

        throw new NotSupportedException($"Unknown language {compiler.Language}");
    }

    private static BaseCodeDomTreeGenerator CreateGenerator(string path)
    {
        var extension = Path.GetExtension(path).ToLower();

        return extension switch
        {
            ".aspx" => CreateFromPage(path),
            ".master" => CreateFromMasterPage(path),
            _ => throw new NotImplementedException($"Unknown extension for compilation: {extension}"),
        };

        static BaseCodeDomTreeGenerator CreateFromPage(string path)
        {
            var parser = new PageParser();
            parser.AddAssemblyDependency(Assembly.GetEntryAssembly(), true);
            parser.Parse(Array.Empty<string>(), path);

            return new PageCodeDomTreeGenerator(parser);
        }
        static BaseCodeDomTreeGenerator CreateFromMasterPage(string path)
        {
            var parser = new MasterPageParser();
            parser.AddAssemblyDependency(Assembly.GetEntryAssembly(), true);
            parser.Parse(Array.Empty<string>(), path);

            return new MasterPageCodeDomTreeGenerator(parser);
        }
    }

    public void Dispose()
    {
        _csharp.Dispose();
        _vb.Dispose();
    }

    private interface ICompiler : IDisposable
    {
        CodeDomProvider Provider { get; }

        SyntaxTree ParseText(SourceText source, string path, CancellationToken cancellationToken);

        Compilation CreateCompilation(string typeName, IEnumerable<SyntaxTree> trees, IEnumerable<MetadataReference> references);

        void IDisposable.Dispose() => Provider.Dispose();
    }

    private sealed class CSharpCompiler : ICompiler
    {
        public CodeDomProvider Provider { get; } = CodeDomProvider.CreateProvider("CSharp");

        public SyntaxTree ParseText(SourceText source, string path, CancellationToken cancellationToken)
            => CSharpSyntaxTree.ParseText(source, path: path, cancellationToken: cancellationToken);

        public Compilation CreateCompilation(string typeName, IEnumerable<SyntaxTree> trees, IEnumerable<MetadataReference> references)
            => CSharpCompilation.Create($"WebForms.{typeName}",
              options: new CSharpCompilationOptions(
                  outputKind: OutputKind.DynamicallyLinkedLibrary,
                  optimizationLevel: OptimizationLevel.Debug),
              syntaxTrees: trees,
              references: references);
    }

    private sealed class VisualBasicCompiler : ICompiler
    {
        private readonly MetadataReference[] _vbReferences;

        public VisualBasicCompiler()
        {
            _vbReferences = new[] { MetadataReference.CreateFromFile(Assembly.Load("Microsoft.VisualBasic.Core").Location) };
        }

        public CodeDomProvider Provider { get; } = CodeDomProvider.CreateProvider("VisualBasic");

        public SyntaxTree ParseText(SourceText source, string path, CancellationToken cancellationToken)
            => VisualBasicSyntaxTree.ParseText(source, path: path, cancellationToken: cancellationToken);

        public Compilation CreateCompilation(string typeName, IEnumerable<SyntaxTree> trees, IEnumerable<MetadataReference> references)
            => VisualBasicCompilation.Create($"WebForms.{typeName}",
              options: new VisualBasicCompilationOptions(
                  outputKind: OutputKind.DynamicallyLinkedLibrary,
                  optimizationLevel: OptimizationLevel.Debug),
              syntaxTrees: trees,
              references: references.Concat(_vbReferences));
    }
}
