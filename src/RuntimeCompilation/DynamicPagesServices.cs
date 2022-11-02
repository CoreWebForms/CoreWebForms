// MIT License.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.SystemWebAdapters.UI.RuntimeCompilation;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

public static class DynamicPagesServices
{
    public static ISystemWebAdapterBuilder AddDynamicPages(this ISystemWebAdapterBuilder services, Action<PageCompilationOptions> configure)
    {
        services.Services.AddSingleton<RoslynPageCompiler>();
        services.Services.AddSingleton<SystemWebCompilation>();
        services.Services.AddSingleton<IPageCompiler>(ctx =>
        {
            if (ctx.GetRequiredService<IOptions<PageCompilationOptions>>() is { Value.UseFrameworkParser: true })
            {
                return ctx.GetRequiredService<SystemWebCompilation>();
            }
            else
            {
                return ctx.GetRequiredService<RoslynPageCompiler>();
            }
        });

        services.Services.AddSingleton<ICompilationRegistrar, CompilationRegistrar>();
        services.Services.AddSingleton<IQueue, ChannelQueue>();
        services.Services.AddHostedService<SerializedCompilation>();
        services.Services.AddTransient<IStartupFilter, ParserSetting>();

        services.Services.AddOptions<PageCompilationOptions>()
            .Configure(configure);

        return services;
    }

    private class ParserSetting : IStartupFilter
    {
        private readonly IOptions<PageCompilationOptions> _options;

        public ParserSetting(IOptions<PageCompilationOptions> options)
        {
            _options = options;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            if (_options.Value.UseFrameworkParser)
            {
                return builder => next(builder);
            }
            else
            {
                return builder =>
                {
                    builder.Use((ctx, next) =>
                    {
                        ctx.Items["Parser"] = "Custom";
                        return next(ctx);
                    });
                    next(builder);
                };
            }
        }
    }
}
