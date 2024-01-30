// MIT License.

using System.CodeDom.Compiler;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Compilation;
using System.Web.UI;
using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace WebForms.Compiler.Dynamic;

internal abstract class SystemWebCompilation : IPageCompiler, IDisposable
{
    private readonly ICompiler _csharp;
    private readonly ICompiler _vb;

    public SystemWebCompilation(
        ILoggerFactory factory,
        IOptions<PagesSection> pagesSection,
        IOptions<CompilationSection> compilationSection)
    {
        _csharp = new CSharpCompiler();
        _vb = new VisualBasicCompiler();

        // TODO: remove these statics and use DI
        MTConfigUtil.Compilation = compilationSection.Value;
        PagesSection.Instance = pagesSection.Value;
    }

    public async Task<ICompiledPage> CompilePageAsync(IFileProvider files, string path, CancellationToken token)
    {
        try
        {
            return new TrackedCompiledPage(this, await InternalCompilePageAsync(files, path, token).ConfigureAwait(false));
        }
        catch (HttpParseException ex)
        {
            return CompiledPage.FromError(new(path), ex);
        }
    }

    private sealed class TrackedCompiledPage(SystemWebCompilation c, ICompiledPage other) : ICompiledPage
    {
        public PathString Path => other.Path;

        public string AspxFile => other.AspxFile;

        public Type? Type => other.Type;

        public Exception? Exception => other.Exception;

        public IReadOnlyCollection<string> FileDependencies => other.FileDependencies;

        public MetadataReference MetadataReference => other.MetadataReference;

        public void Dispose()
        {
            c._compiled.Remove(Path);
            other.Dispose();
        }
    }

    private readonly Dictionary<string, Task<ICompiledPage>> _compiled = new();

    private async Task<ICompiledPage> InternalCompilePageAsync(IFileProvider files, string path, CancellationToken token)
    {
        var queue = new Queue<string>();
        queue.Enqueue(path);

        var embedded = new List<EmbeddedText>();
        var trees = new List<SyntaxTree>();
        string typeName = null!;

        ICompiler compiler = null!;

        var dependencies = new List<ICompiledPage>();

        while (queue.Count > 0)
        {
            var currentPath = queue.Dequeue();
            var generator = CreateGenerator(currentPath);

            dependencies.AddRange(await GetDependencyPages(files, generator.Parser, token).ConfigureAwait(false));

            if (compiler is null)
            {
                compiler = GetProvider(generator.Parser.CompilerType);
            }

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
        }

        var assemblies = dependencies.Select(d => d.Type?.Assembly).Where(a => a is not null);
        var references = GetMetadataReferences().Concat(dependencies.Select(d => d.MetadataReference).Where(d => d is not null));
        var compilation = compiler.CreateCompilation(typeName, trees, references!);

        return CreateCompiledPage(compilation, path, typeName, trees, references!, embedded, assemblies!, token);
    }

    private async Task<ICompiledPage[]> GetDependencyPages(IFileProvider files, TemplateParser parser, CancellationToken token)
    {
        var p = new List<Task<ICompiledPage>>();

        foreach (var path in GetDependencyPaths(parser))
        {
            if (_compiled.TryGetValue(path, out var existing))
            {
                p.Add(existing);
            }
            else
            {
                var task = CompilePageAsync(files, path, token);
                _compiled.Add(path, task);
                p.Add(task);
            }
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

    protected abstract ICompiledPage CreateCompiledPage(
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
}
