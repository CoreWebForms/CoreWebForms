// MIT License.

using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.SystemWebAdapters.UI.RuntimeCompilation;

namespace Microsoft.Extensions.DependencyInjection;

public static class DynamicPagesServices
{
    public static ISystemWebAdapterBuilder AddDynamicPages(this ISystemWebAdapterBuilder services, Action<PageCompilationOptions> configure)
    {
        services.Services.AddSingleton<IPageCompiler, SystemWebCompilation>();
        services.Services.AddSingleton<ICompilationRegistrar, CompilationRegistrar>();
        services.Services.AddSingleton<IQueue, ChannelQueue>();
        services.Services.AddHostedService<SerializedCompilation>();

        services.Services.AddOptions<PageCompilationOptions>()
            .Configure(configure);

        return services;
    }
}
