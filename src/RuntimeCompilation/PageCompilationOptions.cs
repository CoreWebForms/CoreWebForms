// MIT License.

using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Microsoft.AspNetCore.SystemWebAdapters.UI.RuntimeCompilation;

public class PageCompilationOptions
{
    private readonly ControlCollection _controls;

    public PageCompilationOptions()
    {
        _controls = new();

        // Ensure this assembly is loaded
        _ = typeof(HttpUtility).Assembly;

        AddTypeNamespace(typeof(Page), "asp");
        AddTypeNamespace(typeof(TextBox), "asp");
    }

    public IReadOnlyCollection<Assembly> Assemblies => _controls.Assemblies;

    public void AddTypeNamespace(Type type, string prefix)
        => AddAssembly(type.Assembly, type.Namespace ?? throw new InvalidOperationException(), prefix);

    internal void AddAssembly(Assembly assembly, string ns, string prefix)
        => _controls.Add(assembly, ns, prefix);

    private sealed class ControlCollection
    {
        private readonly HashSet<Assembly> _assemblies = new();

        public IReadOnlyCollection<Assembly> Assemblies => _assemblies;

        public void Add(Assembly assembly, string ns, string prefix)
        {
            _assemblies.Add(assembly);
        }
    }
}

