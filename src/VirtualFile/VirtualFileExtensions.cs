// MIT License.

using System.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.AspNetCore.Builder;

public static class VirtualFileExtensions
{
    public static ISystemWebAdapterBuilder AddVirtualPathProvider(this ISystemWebAdapterBuilder builder)
    {
        builder.Services.TryAddSingleton<VirtualPathProvider, ContentRootVirtualPathProvider>();

        return builder;
    }

    private sealed class ContentRootVirtualPathProvider(IWebHostEnvironment env) : FileProviderVirtualPathProvider(env.ContentRootFileProvider)
    {
    }
}
