// MIT License.

using System.CodeDom.Compiler;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Compilation;
using System.Web.UI;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace WebForms.Compiler.Dynamic;

internal sealed class SystemWebCompilation : IDisposable, IWebFormsCompiler
{
    private readonly ICompiler _csharp;
    private readonly ICompiler _vb;
    private readonly IMetadataProvider _metadata;
    private readonly ILoggerFactory _factory;
    private readonly ILogger<SystemWebCompilation> _logger;
    private readonly IOptions<WebFormsOptions> _webFormsOptions;
    private readonly IOptions<PageCompilationOptions> _pageCompilationOptions;
    private readonly string[] _ignoredFolders = new[] { "bin", "obj", "Properties" };

    public SystemWebCompilation(
        ILoggerFactory logger,
        IMetadataProvider metadata,
        IOptions<WebFormsOptions> webFormsOptions,
        IOptions<PageCompilationOptions> pageCompilationOptions)
    {
        _csharp = new CSharpCompiler(pageCompilationOptions.Value);
        _vb = new VisualBasicCompiler(pageCompilationOptions.Value);

        _metadata = metadata;
        _factory = logger;
        _logger = logger.CreateLogger<SystemWebCompilation>();
        _webFormsOptions = webFormsOptions;
        _pageCompilationOptions = pageCompilationOptions;

        _logger.LogInformation("Compiler set to IsDebug={IsDebug}", pageCompilationOptions.Value.IsDebug);
    }

    public IFileProvider Files => _webFormsOptions.Value.WebFormsFileProvider;

    private bool IsIgnoredFolder(VirtualPath path)
    {
        var normalizedPath = path.Path.TrimStart('~').ToLowerInvariant();
        return _ignoredFolders.Any(folder => normalizedPath.StartsWith($"/{folder}"));
    }

    private SystemWebCompilationUnit CompileAllPages(ICompilationStrategy strategy, CancellationToken token)
    {
        var aspxFiles = Files.GetFiles().Where(t => t.FullPath.EndsWith(".aspx"))
            .Select(t => new VirtualPath("/" + t.FullPath))
            .Where(x => !IsIgnoredFolder(x));

        var compilation = new SystemWebCompilationUnit(strategy);

        foreach (var parser in GetParsersToCompile(aspxFiles, compilation))
        {
            compilation[parser.CurrentVirtualPath] = InternalCompilePage(compilation, parser, token);
        }

        return compilation;
    }

    /// <summary>
    /// We need to identify the order for building. Dependencies of a page/control/etc can be retrieved
    /// by using <see cref="TemplateParser.GetDependencyPaths"/>, but we want to ensure we only compile something if its
    /// dependencies have already been compiled.
    /// </summary>
    private IEnumerable<DependencyParser> GetParsersToCompile(IEnumerable<VirtualPath> files, SystemWebCompilationUnit compilationUnit)
    {
        var parsers = new Dictionary<VirtualPath, DependencyParser>();

        var stack = new Stack<VirtualPath>(files);
        var visited = new HashSet<VirtualPath>();

        while (stack.Count > 0)
        {
            var currentPath = stack.Peek();

            if (!parsers.TryGetValue(currentPath, out var parser))
            {
                parsers[currentPath] = parser = CreateParser(currentPath, compilationUnit);
            }

            foreach (var dependency in parser.GetDependencyPaths())
            {
                if (visited.Add(dependency))
                {
                    stack.Push(dependency);
                }
            }

            // If no dependencies were added, the top will still be the current path and we should return it
            if (ReferenceEquals(stack.Peek(), currentPath))
            {
                stack.Pop();
                try
                {
                    parser.Parse();
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to parse {Path}", currentPath);
                }

                yield return parser;
            }
        }
    }

    private CompiledPage InternalCompilePage(SystemWebCompilationUnit compiledPages, DependencyParser parser, CancellationToken token)
    {
        var currentPath = parser.CurrentVirtualPath;

        try
        {
            var embedded = new List<EmbeddedText>();
            var trees = new List<SyntaxTree>();

            var compiler = GetProvider(parser.TemplateParser.CompilerType);
            var generator = parser.TemplateParser.GetGenerator();
            var cu = generator.GetCodeDomTree(compiler.Provider, new StringResourceBuilder(), currentPath);

            using var writer = new StringWriter();
            compiler.Provider.GenerateCodeFromCompileUnit(cu, writer, new());
            var source = SourceText.From(writer.ToString(), Encoding.UTF8);

            var filename = $"__{currentPath}.{compiler.Provider.FileExtension}";
            var sourceTree = compiler.ParseText(source, path: filename, cancellationToken: token);

            trees.Add(sourceTree);
            embedded.Add(EmbeddedText.FromSource(filename, source));

            foreach (var dep in generator.Parser.SourceDependencies)
            {
                if (dep is string p && Files.GetFileInfo(p) is { Exists: true, IsDirectory: false } file)
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

            var dependencies = parser.GetDependencyPaths().Select(p => compiledPages[p]).ToList();
            var assemblies = dependencies.Select(d => d.Type?.Assembly).Where(a => a is not null);
            var references = _metadata.References
                .Concat(dependencies.Select(d => d.MetadataReference).Where(d => d is not null));
            var typeName = generator.GetInstantiatableFullTypeName();

            var compilation = compiler.CreateCompilation(typeName, trees, references!);

            var compiled = CreateCompiledPage(compiledPages, compilation, currentPath, typeName, embedded, assemblies!, token);

            _logger.LogInformation("Compiled {Path}", currentPath);

            return compiled;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to compile {Path}", currentPath);

            if (compiledPages.Strategy.HandleExceptions)
            {
                return new(currentPath)
                {
                    Exception = e
                };
            }
            else
            {
                throw;
            }
        }
    }

    private CompiledPage CreateCompiledPage(
        SystemWebCompilationUnit cu,
        Compilation compilation,
        VirtualPath virtualPath,
        string typeName,
        IEnumerable<EmbeddedText> embedded,
        IEnumerable<Assembly> assemblies,
        CancellationToken token)
    {
        using var peStream = cu.Strategy.CreatePeStream(virtualPath.Path, typeName, compilation.AssemblyName!);
        using var pdbStream = cu.Strategy.CreatePdbStream(virtualPath.Path, typeName, compilation.AssemblyName!);

        var result = compilation.Emit(
            embeddedTexts: embedded,
            peStream: peStream,
            pdbStream: pdbStream,
            cancellationToken: token);

        if (!result.Success)
        {
            IEnumerable<Diagnostic> errors = result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error);
            IEnumerable<Diagnostic> warnings = result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning);

            _logger.LogError("{ErrorCount} error(s) found compiling {Route}", errors.Count(), virtualPath);
            _logger.LogWarning("{WarningCount} warning(s) found compiling {Route}", warnings.Count(), virtualPath);

            if (!cu.Strategy.HandleErrors(virtualPath, result.Diagnostics))
            {
                throw new RoslynCompilationException(virtualPath, result.Diagnostics.ConvertToErrors());
            }
            else
            {
                return new(virtualPath)
                {
                    Exception = new RoslynCompilationException(virtualPath, result.Diagnostics.ConvertToErrors())
                };
            }
        }
        else
        {
            peStream.Position = 0;
            pdbStream.Position = 0;

            var context = new PageAssemblyLoadContext(virtualPath, assemblies, _factory.CreateLogger<PageAssemblyLoadContext>());
            var assembly = context.LoadFromStream(peStream, pdbStream);

            peStream.Position = 0;

            if (assembly.GetType(typeName) is Type type)
            {
                return new CompiledPage(virtualPath)
                {
                    MetadataReference = MetadataReference.CreateFromStream(peStream),
                    Type = type,
                };
            }
        }

        throw new InvalidOperationException("No type found");
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

    private DependencyParser CreateParser(VirtualPath path, SystemWebCompilationUnit compilationUnit)
    {
        var extension = path.Extension;

        if (_pageCompilationOptions.Value.Parsers.TryGetValue(extension, out var parser))
        {
            return parser(path, compilationUnit);
        }

        throw new NotImplementedException($"Unknown extension for compilation: {extension}");
    }

    public void Dispose()
    {
        _csharp.Dispose();
        _vb.Dispose();
    }

    ICompilationResult IWebFormsCompiler.CompilePages(ICompilationStrategy outputProvider, CancellationToken token)
    {
        var result = CompileAllPages(outputProvider, token);
        return result.Build();
    }

    private interface ICompiler : IDisposable
    {
        CodeDomProvider Provider { get; }

        SyntaxTree ParseText(SourceText source, string path, CancellationToken cancellationToken);

        Compilation CreateCompilation(string typeName, IEnumerable<SyntaxTree> trees, IEnumerable<MetadataReference> references);

        void IDisposable.Dispose() => Provider.Dispose();
    }

    private static TOptions CreateOptions<TOptions>(PageCompilationOptions pageOptions, TOptions options)
        where TOptions : CompilationOptions
    {
        var result = options.WithOptimizationLevel(pageOptions.IsDebug ? OptimizationLevel.Debug : OptimizationLevel.Release);

        if (pageOptions.OnCreateOption is { } onCreate)
        {
            result = onCreate(options);
        }

        return (TOptions)result;
    }

    private sealed class CSharpCompiler(PageCompilationOptions options) : ICompiler
    {
        public CodeDomProvider Provider { get; } = CodeDomProvider.CreateProvider("CSharp");

        public SyntaxTree ParseText(SourceText source, string path, CancellationToken cancellationToken)
            => CSharpSyntaxTree.ParseText(source, path: path, cancellationToken: cancellationToken);

        public Compilation CreateCompilation(string typeName, IEnumerable<SyntaxTree> trees, IEnumerable<MetadataReference> references)
            => CSharpCompilation.Create($"WebForms.{typeName}",
              options: CreateOptions(options, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)),
              syntaxTrees: trees,
              references: references);
    }

    private sealed class VisualBasicCompiler : ICompiler
    {
        private readonly PageCompilationOptions _options;
        private readonly MetadataReference[] _vbReferences;

        public VisualBasicCompiler(PageCompilationOptions options)
        {
            _options = options;
            _vbReferences = new[] { MetadataReference.CreateFromFile(Assembly.Load("Microsoft.VisualBasic.Core").Location) };
        }

        public CodeDomProvider Provider { get; } = CodeDomProvider.CreateProvider("VisualBasic");

        public SyntaxTree ParseText(SourceText source, string path, CancellationToken cancellationToken)
            => VisualBasicSyntaxTree.ParseText(source, path: path, cancellationToken: cancellationToken);

        public Compilation CreateCompilation(string typeName, IEnumerable<SyntaxTree> trees, IEnumerable<MetadataReference> references)
            => VisualBasicCompilation.Create($"WebForms.{typeName}",
              options: CreateOptions(_options, new VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary)),
              syntaxTrees: trees,
              references: references.Concat(_vbReferences));
    }
}
