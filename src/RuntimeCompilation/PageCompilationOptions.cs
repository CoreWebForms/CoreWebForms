// MIT License.

using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Microsoft.AspNetCore.SystemWebAdapters.UI.RuntimeCompilation;

public class PageCompilationOptions
{
    private readonly HashSet<Assembly> _assemblies;

    internal ICollection<TagNamespaceRegisterEntry> KnownTags { get; }

    public PageCompilationOptions()
    {
        _assemblies = new HashSet<Assembly>();
        KnownTags = new List<TagNamespaceRegisterEntry>();

        // Ensure this assembly is loaded
        _ = typeof(HttpUtility).Assembly;

        AddTypeNamespace(typeof(Page), "asp");
        AddTypeNamespace(typeof(TextBox), "asp");
    }

    public IEnumerable<Assembly> Assemblies => _assemblies;

    public void AddTypeNamespace(Type type, string prefix)
        => AddAssembly(type.Assembly, type.Namespace ?? throw new InvalidOperationException(), prefix);

    internal void AddAssembly(Assembly assembly, string ns, string prefix)
    {
        KnownTags.Add(new(prefix, ns, assembly.FullName));
        _assemblies.Add(assembly);
    }
}

