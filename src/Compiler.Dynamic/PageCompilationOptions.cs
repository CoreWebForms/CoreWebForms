// MIT License.

using System.Reflection;
using System.Web;
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
