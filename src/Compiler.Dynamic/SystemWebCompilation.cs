// MIT License.

using System.CodeDom.Compiler;
using System.Collections.Immutable;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Compilation;
using System.Web.UI;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebForms.Internal;

namespace WebForms.Compiler.Dynamic;

internal abstract class SystemWebCompilation<T> : IDisposable
    where T : ICompiledPage
{
    private readonly ICompiler _csharp;
    private readonly ICompiler _vb;
    private readonly IMetadataProvider _metadata;
    private readonly ILogger<SystemWebCompilation<T>> _logger;
    private readonly IOptions<WebFormsOptions> _webFormsOptions;
    private readonly IOptions<PageCompilationOptions> _pageCompilationOptions;

    private SystemWebCompilationUnit _compiled = new();

    private sealed class SystemWebCompilationUnit : ICompiledTypeAccessor
    {
        private readonly Dictionary<string, T> _cache = [];
        private readonly Dictionary<string, Type> _typeMap = [];

        public IEnumerable<T> Values => _cache.Values;

        public T this[string path]
        {
            get => _cache[path];
            set
            {
                _cache[path] = value;

                if (value.Type is { } type)
                {
                    _typeMap[path] = type;
                }
                else if (value is null)
                {
                    _typeMap.Remove(path);
                }
            }
        }

        Type? ICompiledTypeAccessor.GetForPath(string virtualPath)
            => _cache.TryGetValue(virtualPath, out var page) && page.Type is { } type ? type : null;

        Type? ICompiledTypeAccessor.GetForName(string typeName)
            => _typeMap.TryGetValue(typeName, out var type) ? type : null;
    }

    public SystemWebCompilation(
        ILoggerFactory logger,
        IMetadataProvider metadata,
        IOptions<WebFormsOptions> webFormsOptions,
        IOptions<PageCompilationOptions> pageCompilationOptions)
    {
        _csharp = new CSharpCompiler(pageCompilationOptions.Value);
        _vb = new VisualBasicCompiler(pageCompilationOptions.Value);

        _metadata = metadata;
        _logger = logger.CreateLogger<SystemWebCompilation<T>>();
        _webFormsOptions = webFormsOptions;
        _pageCompilationOptions = pageCompilationOptions;

        _logger.LogInformation("Compiler set to IsDebug={IsDebug}", pageCompilationOptions.Value.IsDebug);
    }

    protected ICompiledTypeAccessor TypeAccessor => _compiled;

    protected IEnumerable<T> GetPages()
    {
        foreach (var page in _compiled.Values)
        {
            if (page.AspxFile.EndsWith(".aspx", StringComparison.OrdinalIgnoreCase))
            {
                yield return page;
            }
        }
    }

    protected IDisposable MarkRecompile()
    {
        var before = _compiled;

        _compiled = new();

        return new DelegateDisposable(() =>
        {
            foreach (var page in before.Values)
            {
                page.Dispose();
            }
        });
    }

    public IFileProvider Files => _webFormsOptions.Value.WebFormsFileProvider;

    protected void CompileAllPages(CancellationToken token)
    {
        var aspxFiles = Files.GetFiles().Where(t => t.FullPath.EndsWith(".aspx"))
            .Select(t => t.FullPath);
        var compilation = new SystemWebCompilationUnit();

        foreach (var parser in GetParsersToCompile(aspxFiles, compilation))
        {
            compilation[parser.CurrentVirtualPath.Path] = InternalCompilePage(compilation, parser, token);
        }

        _compiled = compilation;
    }

    /// <summary>
    /// We need to identify the order for building. Dependencies of a page/control/etc can be retrieved
    /// by using <see cref="TemplateParser.GetDependencyPaths"/>, but we want to ensure we only compile something if its
    /// dependencies have already been compiled.
    /// </summary>
    private IEnumerable<DependencyParser> GetParsersToCompile(IEnumerable<string> files, SystemWebCompilationUnit compilationUnit)
    {
        var parsers = new Dictionary<string, DependencyParser>();

        var stack = new Stack<string>(files);
        var visited = new HashSet<string>();

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
                parser.Parse();
                yield return parser;
            }
        }
    }

    private T InternalCompilePage(SystemWebCompilationUnit compiledPages, DependencyParser parser, CancellationToken token)
    {
        var currentPath = parser.CurrentVirtualPath.Path;

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

            var compiled = CreateCompiledPage(compilation, currentPath, typeName, trees, references!, embedded, assemblies!, token);

            foreach (var dependency in dependencies)
            {
                dependency.PageDependencies.Add(compiled);
            }

            _logger.LogInformation("Compiled {Path}", currentPath);

            return compiled;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to compile {Path}", currentPath);
            return CreateErrorPage(currentPath, e);
        }
    }

    protected abstract T CreateErrorPage(string path, Exception e);

    protected abstract T CreateCompiledPage(
        Compilation compilation,
        string route,
        string typeName,
        IEnumerable<SyntaxTree> trees,
        IEnumerable<MetadataReference> references,
        IEnumerable<EmbeddedText> embedded,
        IEnumerable<Assembly> assemblies,
        CancellationToken token);

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

    private DependencyParser CreateParser(string path, SystemWebCompilationUnit compilationUnit)
    {
        var extension = Path.GetExtension(path);

        if (_pageCompilationOptions.Value.Parsers.TryGetValue(extension, out var parser))
        {
            //check with Taylor why _compiled is set after compilation is done, else below could be simplified.
            //return parser(path, _compiled);

            return parser(path, compilationUnit);
        }

        throw new NotImplementedException($"Unknown extension for compilation: {extension}");
    }

    protected IEnumerable<RoslynError> GetErrors(ImmutableArray<Diagnostic> diagnostics)
    {
        foreach (var d in diagnostics)
        {
            var logLevel = d.Severity switch
            {
                DiagnosticSeverity.Hidden => LogLevel.Trace,
                DiagnosticSeverity.Info => LogLevel.Debug,
                DiagnosticSeverity.Warning => LogLevel.Warning,
                DiagnosticSeverity.Error => LogLevel.Error,
                _ => LogLevel.Critical,
            };

            _logger.Log(logLevel, "[{Id}] {Message} @{Location}", d.Id, d.GetMessage(CultureInfo.CurrentCulture), d.Location);

            yield return new RoslynError()
            {
                Id = d.Id,
                Message = d.GetMessage(CultureInfo.CurrentCulture),
                Severity = d.Severity,
                Location = d.Location.ToString(),
            };
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

    private sealed class CSharpCompiler(PageCompilationOptions options) : ICompiler
    {
        public CodeDomProvider Provider { get; } = CodeDomProvider.CreateProvider("CSharp");

        public SyntaxTree ParseText(SourceText source, string path, CancellationToken cancellationToken)
            => CSharpSyntaxTree.ParseText(source, path: path, cancellationToken: cancellationToken);

        public Compilation CreateCompilation(string typeName, IEnumerable<SyntaxTree> trees, IEnumerable<MetadataReference> references)
            => CSharpCompilation.Create($"WebForms.{typeName}",
              options: new CSharpCompilationOptions(
                  outputKind: OutputKind.DynamicallyLinkedLibrary,
                  optimizationLevel: options.IsDebug ? OptimizationLevel.Debug : OptimizationLevel.Release),
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
              options: new VisualBasicCompilationOptions(
                  outputKind: OutputKind.DynamicallyLinkedLibrary,
                  optimizationLevel: _options.IsDebug ? OptimizationLevel.Debug : OptimizationLevel.Release),
              syntaxTrees: trees,
              references: references.Concat(_vbReferences));
    }

    private sealed class DelegateDisposable(Action action) : IDisposable
    {
        public void Dispose() => action();
    }
}
