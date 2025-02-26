// MIT License.

using System.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Builder;

public static class VirtualFileExtensions
{
    public static ISystemWebAdapterBuilder AddVirtualPathProvider(this ISystemWebAdapterBuilder builder)
    {
        builder.Services.AddTransient<ContentRootVirtualPathProvider>();
        builder.Services.AddOptions<SystemWebAdaptersOptions>()
            .Configure<ContentRootVirtualPathProvider>((options, provider) =>
        {
            options.VirtualPathProvider = provider;
        });

        return builder;
    }

    private sealed class ContentRootVirtualPathProvider(IWebHostEnvironment env) : FileProviderVirtualPathProvider(env.ContentRootFileProvider)
    {
    }
}
