// MIT License.

using System.Reflection;
using System.Web;
using System.Web.UI;

// Load up types to include
var toInclude = new[]
{
    typeof(HttpContext).Assembly,
    typeof(IHttpHandler).Assembly,
    typeof(Page).Assembly,
}
.SelectMany(a => a.GetTypes())
.Where(t => t.IsPublic)
.Select(t => t.FullName)
.ToHashSet();

var dir = Path.Combine(AppContext.BaseDirectory, "netfx");
using var frameworkContext = new MetadataLoadContext(new PathAssemblyResolver(Directory.EnumerateFiles(dir, "*.dll")));

var forwardedTypes = frameworkContext.LoadFromAssemblyName("System.Web")
    .ExportedTypes
    .Select(t => t.FullName)
    .Where(toInclude.Contains)
    .OrderBy(t => t);

var file = Path.Combine(GetRoot(), "src", "Shim", "Forwards.cs");

using var fs = File.OpenWrite(file);
fs.SetLength(0);
using var writer = new StreamWriter(fs);

writer.WriteLine("// MIT");
writer.WriteLine("using System.Runtime.CompilerServices;");
writer.WriteLine();

foreach (var type in forwardedTypes)
{
    writer.WriteLine($"[assembly: TypeForwardedTo(typeof({type}))]");
}

static string GetRoot()
{
    return GetRoot(new DirectoryInfo(AppContext.BaseDirectory));

    string GetRoot(DirectoryInfo? path)
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

