// MIT License.

using System.CodeDom.Compiler;
using System.Collections.Immutable;
using System.Diagnostics;
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
    private readonly IMetadataProvider _metadata;
    private readonly ILogger<SystemWebCompilation<T>> _logger;
    private readonly IOptions<WebFormsOptions> _webFormsOptions;
    private readonly IOptions<PageCompilationOptions> _pageCompilationOptions;

    private Dictionary<string, Task<T>> _compiled = [];

    public SystemWebCompilation(
        IHostEnvironment env,
        ILoggerFactory logger,
        IMetadataProvider metadata,
        IOptions<WebFormsOptions> webFormsOptions,
        IOptions<PageCompilationOptions> pageCompilationOptions)
    {
        _env = env;
        _csharp = new CSharpCompiler(pageCompilationOptions.Value);
        _vb = new VisualBasicCompiler(pageCompilationOptions.Value);

        _metadata = metadata;
        _logger = logger.CreateLogger<SystemWebCompilation<T>>();
        _webFormsOptions = webFormsOptions;
        _pageCompilationOptions = pageCompilationOptions;

        _logger.LogInformation("Compiler set to IsDebug={IsDebug}", pageCompilationOptions.Value.IsDebug);
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

    public IFileProvider Files => _webFormsOptions.Value.WebFormsFileProvider ?? _env.ContentRootFileProvider;

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
            _logger.LogDebug("Starting compilation for '{Page}'", path);

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
            var references = _metadata.References
                .Concat(dependencies.Select(d => d.MetadataReference).Where(d => d is not null));

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
            _logger.LogError(e, "Failed to compile {Path}", path);
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

        if (_pageCompilationOptions.Value.Parsers.TryGetValue(extension, out var parser))
        {
            return parser(path);
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
