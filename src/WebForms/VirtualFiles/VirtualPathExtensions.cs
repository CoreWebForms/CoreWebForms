// MIT License.

#nullable enable

namespace System.Web.Hosting;

internal static class VirtualPathExtensions
{
    public static VirtualPath CombineVirtualPathsInternal(this VirtualPathProvider? provider, VirtualPath basePath, VirtualPath relativePath)
    {
        if (provider is { })
        {
            return provider.CombineVirtualPaths(basePath, relativePath);
        }

        return basePath.Parent.Combine(relativePath);
    }
}
