// MIT License.

using System.Globalization;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Routing;
using System.Web.UI;

var assemblies = new[] {
    ("System.Web", "Shim"),
    ("System.Web.Extensions", "Shim.Extensions")
};

// Load up types to include
var toInclude = new[]
{
    typeof(HttpContext).Assembly,
    typeof(HtmlTextWriter).Assembly,
    typeof(IHttpHandler).Assembly,
    typeof(RouteCollection).Assembly,
    typeof(VirtualFile).Assembly,
    typeof(Page).Assembly,
    typeof(ScriptManager).Assembly,
}
.SelectMany(a => a.GetTypes())
.Where(t => t.IsPublic)
.Select(t => t.FullName)
.ToHashSet();

foreach (var (netFxAssembly, shimProject) in assemblies)
{
    var dir = Path.Combine(AppContext.BaseDirectory, "netfx");
    using var frameworkContext = new MetadataLoadContext(new PathAssemblyResolver(Directory.EnumerateFiles(dir, "*.dll")));

    var forwardedTypes = frameworkContext.LoadFromAssemblyName(netFxAssembly)
        .ExportedTypes
        .Where(t => toInclude.Contains(t.FullName))
        .Select(type =>
        {
            var idx = type.FullName!.IndexOf('`');

            if (idx == -1)
            {
                return type.FullName;
            }

            var sb = new StringBuilder();
            sb.Append(type.FullName.AsSpan(0, idx));
            sb.Append('<');

            var count = int.Parse(type.FullName.AsSpan(idx + 1), NumberStyles.Integer, CultureInfo.InvariantCulture);
            sb.Append(',', count - 1);

            sb.Append('>');

            return sb.ToString();
        })
        .OrderBy(t => t);

    var file = Path.Combine(GetRoot(), "src", shimProject, "Forwards.cs");

    using var fs = File.OpenWrite(file);
    fs.SetLength(0);
    using var writer = new StreamWriter(fs);

    writer.WriteLine("// MIT License.");
    writer.WriteLine();
    writer.WriteLine("using System.Runtime.CompilerServices;");
    writer.WriteLine();

    foreach (var type in forwardedTypes)
    {
        writer.WriteLine($"[assembly: TypeForwardedTo(typeof({type}))]");
    }

    static string GetRoot()
    {
        return GetRoot(new DirectoryInfo(AppContext.BaseDirectory));

        static string GetRoot(DirectoryInfo? path)
        {
            if (path is null)
            {
                throw new InvalidOperationException();
            }

            if (Directory.Exists(Path.Combine(path.FullName, ".git")))
            {
                return path.FullName;
            }

            return GetRoot(path.Parent);
        }
    }
}
