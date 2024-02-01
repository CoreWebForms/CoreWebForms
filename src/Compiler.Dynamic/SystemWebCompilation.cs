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
using Microsoft.Extensions.Options;

namespace WebForms.Compiler.Dynamic;

internal abstract class SystemWebCompilation<T> : IDisposable
    where T : ICompiledPage
{
    private readonly ICompiler _csharp;
    private readonly ICompiler _vb;
    private readonly IOptions<PageCompilationOptions> _pageCompilation;

    private Dictionary<string, Task<T>> _compiled = [];

    public SystemWebCompilation(
        IOptions<PageCompilationOptions> pageCompilation,
        IOptions<PagesSection> pagesSection,
        IOptions<CompilationSection> compilationSection)
    {
        _csharp = new CSharpCompiler();
        _vb = new VisualBasicCompiler();

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

    public IFileProvider Files => _pageCompilation.Value.Files;

    protected void RemovePage(string path) => _compiled.Remove(path);

    protected Task<T> CompilePageAsync(string path, CancellationToken token)
    {
        if (_compiled.TryGetValue(path, out var result))
        {
            return result;
        }
        else
        {
            var task = InternalCompilePageAsync(path, token);

            _compiled.Add(path, task);

            return task;
        }
    }

    private async Task<T> InternalCompilePageAsync(string path, CancellationToken token)
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
                if (dep is string p && _pageCompilation.Value.Files.GetFileInfo(p) is { Exists: true, IsDirectory: false } file)
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

        return compiled;
    }

    private async Task<T[]> GetDependencyPages(TemplateParser parser, CancellationToken token)
    {
        var p = new List<Task<T>>();

        foreach (var path in GetDependencyPaths(parser))
        {
            p.Add(CompilePageAsync(path, token));
        }

        return await Task.WhenAll(p).ConfigureAwait(false);

        static IEnumerable<string> GetDependencyPaths(TemplateParser parser)
        {
            if (parser is PageParser { MasterPage.Path: { } masterPage })
            {
                yield return masterPage;
            }
        }
    }

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

    private static BaseCodeDomTreeGenerator CreateGenerator(string path)
    {
        var extension = Path.GetExtension(path).ToLower();

        return extension switch
        {
            ".aspx" => CreateFromPage(path),
            //".ascx" => CreateFromUserControl(path),
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

        //TODO https://github.com/twsouthwick/systemweb-adapters-ui/issues/19 , keeping the code to tackle in next CR
#pragma warning disable CS8321 // Local function is declared but never used
        static BaseCodeDomTreeGenerator CreateFromUserControl(string path)
        {
            var parser = new UserControlParser();
            parser.AddAssemblyDependency(Assembly.GetEntryAssembly(), true);
            parser.Parse(Array.Empty<string>(), path);
            return new UserControlCodeDomTreeGenerator(parser);
        }
#pragma warning restore CS8321 // Local function is declared but never used
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
