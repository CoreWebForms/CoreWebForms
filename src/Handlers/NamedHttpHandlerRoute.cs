// MIT License.

namespace Microsoft.AspNetCore.SystemWebAdapters.HttpHandlers;

/// <summary>
/// A named route for use with <see cref="System.Web.IHttpHandler"/>
/// </summary>
/// <param name="Name">The name of the route.</param>
/// <param name="Route">The route that the named route is mapped to.</param>
/// <param name="Path">The original handler path that should be used.</param>
public sealed record NamedHttpHandlerRoute(string Name, string Route, string Path)
{
    public string Path = EnsureStartsWithSlash(Path);

    private static string EnsureStartsWithSlash(string path)
    {
        if (path.StartsWith('/'))
        {
            return path;
        }

        return "/" + path;
    }
}
