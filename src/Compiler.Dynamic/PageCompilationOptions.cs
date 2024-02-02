// MIT License.

using System.Reflection;
using System.Web;
using System.Web.Compilation;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Extensions.FileProviders;

namespace WebForms.Compiler.Dynamic;

public class PageCompilationOptions
{
    private readonly HashSet<Assembly> _assemblies;

    public IFileProvider Files { get; set; } = new NullFileProvider();

    internal ICollection<TagNamespaceRegisterEntry> KnownTags { get; }

    public PageCompilationOptions()
    {
        _assemblies = new();
        KnownTags = new List<TagNamespaceRegisterEntry>();

        AddTypeNamespace<Page>("asp");
        AddTypeNamespace<TextBox>("asp");

        AddAssembly(typeof(HttpUtility).Assembly);
        AddAssembly(typeof(IHttpHandler).Assembly);
        AddAssembly(typeof(HttpContext).Assembly);
        AddAssembly(typeof(HtmlTextWriter).Assembly);
    }

    internal Dictionary<string, Func<string, BaseCodeDomTreeGenerator>> Parsers { get; } = new(StringComparer.OrdinalIgnoreCase);

    internal void AddParser<TParser>(string extension)
        where TParser : BaseTemplateParser, new()
    {
        Parsers.Add(extension, Create);

        static BaseCodeDomTreeGenerator Create(string path)
        {
            var parser = new TParser();

            parser.AddAssemblyDependency(Assembly.GetEntryAssembly(), true);
            parser.Parse(Array.Empty<string>(), path);

            return parser.GetGenerator();
        }
    }

    public void AddAssembly(Assembly assembly) => _assemblies.Add(assembly);

    public void AddAssemblyFrom<T>() => _assemblies.Add(typeof(T).Assembly);

    public IEnumerable<Assembly> Assemblies => _assemblies;

    public void AddTypeNamespace<T>(string prefix)
        where T : Control
        => AddAssembly(typeof(T).Assembly, typeof(T).Namespace ?? throw new InvalidOperationException(), prefix);

    internal void AddAssembly(Assembly assembly, string ns, string prefix)
    {
        _assemblies.Add(assembly);
        KnownTags.Add(new(prefix, ns, assembly.FullName));
    }
}
