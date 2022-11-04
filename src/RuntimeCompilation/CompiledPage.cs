// MIT License.

using System.Runtime.Loader;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.SystemWebAdapters.UI.RuntimeCompilation;

internal sealed class CompiledPage : ICompiledPage
{
    public CompiledPage(PagePath path, string[] dependencies)
    {
        Path = path.UrlPath;
        FileDependencies = dependencies;
        AspxFile = path.FilePath;
    }

    public Type? Type { get; set; }

    public Memory<byte> Error { get; set; }

    public PathString Path { get; }

    public IReadOnlyCollection<string> FileDependencies { get; }

    public string AspxFile { get; }

    public void Dispose()
    {
        if (Type is not null)
        {
            var type = Type;
            Type = null;
            RemovePage(type);
        }
    }

    private static void RemovePage(Type type)
    {
        var alc = AssemblyLoadContext.GetLoadContext(type.Assembly);

        if (alc is not PageAssemblyLoadContext)
        {
            throw new InvalidOperationException("Tried to unload something that is not a page");
        }

        alc.Unload();
    }
}
