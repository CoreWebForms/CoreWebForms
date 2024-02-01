// MIT License.

using System.Web.UI;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.SystemWebAdapters.HttpHandlers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using WebForms.Compiler.Dynamic;

namespace Microsoft.Extensions.DependencyInjection;

public static class WebFormsCompilerExtensions
{
    public static void AddWebFormsCompilation(this IServiceCollection services, Action<PageCompilationOptions> configure)
    {
        services.AddWebFormsCompilationCore(configure);
        services.AddSingleton<PersistentSystemWebCompilation>();
        services.AddSingleton<IWebFormsCompiler>(ctx => ctx.GetRequiredService<PersistentSystemWebCompilation>());
    }

    public static ISystemWebAdapterBuilder AddDynamicWebForms(this ISystemWebAdapterBuilder services)
        => services.AddDynamicWebForms(options => { });

    public static ISystemWebAdapterBuilder AddDynamicWebForms(this ISystemWebAdapterBuilder services, Action<PageCompilationOptions> configure)
    {
        services.Services.AddWebFormsCompilationCore(configure);
        services.Services.AddHostedService<WebFormsCompilationService>();
        services.Services.AddSingleton<DynamicSystemWebCompilation>();
        services.Services.AddSingleton<IWebFormsCompiler>(ctx => ctx.GetRequiredService<DynamicSystemWebCompilation>());
        services.Services.AddSingleton<IHttpHandlerManager>(ctx => ctx.GetRequiredService<DynamicSystemWebCompilation>());

        return services;
    }

    private static void AddWebFormsCompilationCore(this IServiceCollection services, Action<PageCompilationOptions> configure)
    {
        services.AddOptions<PageCompilationOptions>()
            .Configure<IHostEnvironment>((options, env) =>
            {
                options.Files = env.ContentRootFileProvider;
            })
            .Configure(configure);

        services.AddOptions<PagesSection>()
            .Configure<IOptions<PageCompilationOptions>>((options, compilation) =>
            {
                foreach (var known in compilation.Value.KnownTags)
                {
                    options.DefaultTagNamespaceRegisterEntries.Add(known);
                }

                options.EnableSessionState = System.Web.Configuration.PagesEnableSessionState.True;
            });
    }
}
