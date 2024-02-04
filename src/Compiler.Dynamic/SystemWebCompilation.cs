// MIT License.

using System.CodeDom.Compiler;
using System.Diagnostics;
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
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace WebForms.Compiler.Dynamic;

internal abstract class SystemWebCompilation<T> : IDisposable
    where T : ICompiledPage
{
    private readonly IHostEnvironment _env;
    private readonly ICompiler _csharp;
    private readonly ICompiler _vb;
    private readonly ILogger<SystemWebCompilation<T>> _logger;
    private readonly IOptions<PageCompilationOptions> _pageCompilation;

    private Dictionary<string, Task<T>> _compiled = [];

    public SystemWebCompilation(
        IHostEnvironment env,
        ILoggerFactory logger,
        IOptions<PageCompilationOptions> pageCompilation,
        IOptions<PagesSection> pagesSection,
        IOptions<CompilationSection> compilationSection)
    {
        _env = env;
        _csharp = new CSharpCompiler();
        _vb = new VisualBasicCompiler();

        _logger = logger.CreateLogger<SystemWebCompilation<T>>();
        _pageCompilation = pageCompilation;

        // TODO: remove these statics and use DI
        MTConfigUtil.Compilation = compilationSection.Value;
        PagesSection.Instance = pagesSection.Value;
    }

    protected IEnumerable<T> GetPages()
    {
        foreach (var task in _compiled.Values)
        {
            Debug.Assert(task.IsCompleted);

            var page = task.GetAwaiter().GetResult();

            if (page.AspxFile.EndsWith(".aspx", StringComparison.OrdinalIgnoreCase))
            {
                yield return page;
            }
        }
    }

    protected IDisposable MarkRecompile()
    {
        var before = _compiled;

        _compiled = [];

        return new DelegateDisposable(() =>
        {
            foreach (var page in before.Values)
            {
                Debug.Assert(page.IsCompleted);
                page.GetAwaiter().GetResult().Dispose();
            }
        });
    }

    public IFileProvider Files => _env.ContentRootFileProvider;

    protected void RemovePage(string path) => _compiled.Remove(path);

    protected Task<T> CompilePageAsync(string path, CancellationToken token)
    {
        if (_compiled.TryGetValue(path, out var result))
        {
            _logger.LogDebug("Retrieving previously compiled page '{Page}'", path);
            return result;
        }
        else
        {
            _logger.LogDebug("Enqueueing page for compilation '{Page}'", path);

            var task = InternalCompilePageAsync(path, token);

            _compiled.Add(path, task);

            return task;
        }
    }

    private async Task<T> InternalCompilePageAsync(string path, CancellationToken token)
    {
        try
        {
            var queue = new Queue<string>();
            queue.Enqueue(path);

            var embedded = new List<EmbeddedText>();
            var trees = new List<SyntaxTree>();
            string typeName = null!;

            ICompiler compiler = null!;

            var dependencies = new List<T>();

            while (queue.Count > 0)
            {
                var currentPath = queue.Dequeue();
                var generator = CreateGenerator(currentPath);

                dependencies.AddRange(await GetDependencyPages(generator.Parser, token).ConfigureAwait(false));

                compiler ??= GetProvider(generator.Parser.CompilerType);

                var cu = generator.GetCodeDomTree(compiler.Provider, new StringResourceBuilder(), currentPath);

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
            }

            var assemblies = dependencies.Select(d => d.Type?.Assembly).Where(a => a is not null);
            var references = GetMetadataReferences().Concat(dependencies.Select(d => d.MetadataReference).Where(d => d is not null));
            var compilation = compiler.CreateCompilation(typeName, trees, references!);

            var compiled = CreateCompiledPage(compilation, path, typeName, trees, references!, embedded, assemblies!, token);

            foreach (var dependency in dependencies)
            {
                dependency.PageDependencies.Add(compiled);
            }

            _logger.LogInformation("Compiled {Path}", path);

            return compiled;
        }
        catch (Exception e)
        {
            return CreateErrorPage(path, e);
        }
    }

    private async Task<T[]> GetDependencyPages(TemplateParser parser, CancellationToken token)
    {
        var p = new List<Task<T>>();

        foreach (var path in parser.GetDependencyPaths())
        {
            p.Add(CompilePageAsync(path, token));
        }

        return await Task.WhenAll(p).ConfigureAwait(false);
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

    protected abstract IEnumerable<MetadataReference> GetMetadataReferences();

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

    private BaseCodeDomTreeGenerator CreateGenerator(string path)
    {
        var extension = Path.GetExtension(path);

        if (_pageCompilation.Value.Parsers.TryGetValue(extension, out var parser))
        {
            return parser(path);
        }

        throw new NotImplementedException($"Unknown extension for compilation: {extension}");
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

    private sealed class DelegateDisposable(Action action) : IDisposable
    {
        public void Dispose() => action();
    }
}
