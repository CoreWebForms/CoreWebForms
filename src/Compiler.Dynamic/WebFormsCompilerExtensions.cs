// MIT License.

using System.ComponentModel.Design;
using System.Reflection;
using System.Web.UI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SystemWebAdapters.HttpHandlers;
using Microsoft.Extensions.Options;
using WebForms.Compiler.Dynamic;

namespace Microsoft.Extensions.DependencyInjection;

public static class WebFormsCompilerExtensions
{
    public static IWebFormsBuilder AddPersistentWebFormsCompilation(this IWebFormsBuilder builder, IEnumerable<string> paths)
    {
        builder.Services.AddSingleton<StaticControlCollection>(_ => new StaticControlCollection(paths));
        builder.Services.AddSingleton<ITypeResolutionService>(ctx => ctx.GetRequiredService<StaticControlCollection>());
        builder.Services.AddSingleton<IMetadataProvider>(ctx => ctx.GetRequiredService<StaticControlCollection>());
        builder.Services.AddWebFormsCompilationCore(_ => { });
        builder.Services.AddSingleton<StaticSystemWebCompilation>();
        builder.Services.AddSingleton<IWebFormsCompiler>(ctx => ctx.GetRequiredService<StaticSystemWebCompilation>());

        return builder;
    }

    public static IWebFormsBuilder AddDynamicPages(this IWebFormsBuilder services)
        => services.AddDynamicPages(_ => { });

    public static IWebFormsBuilder AddDynamicPages(this IWebFormsBuilder services, Action<PageCompilationOptions> configure)
    {
        services.Services.AddSingleton<DynamicControlCollection>();
        services.Services.AddSingleton<ITypeResolutionService>(ctx => ctx.GetRequiredService<DynamicControlCollection>());
        services.Services.AddSingleton<IMetadataProvider>(ctx => ctx.GetRequiredService<DynamicControlCollection>());
        services.Services.AddWebFormsCompilationCore(configure);
        services.Services.AddHostedService<WebFormsCompilationService>();
        services.Services.AddSingleton<DynamicSystemWebCompilation>();
        services.Services.AddSingleton<IWebFormsCompiler>(ctx => ctx.GetRequiredService<DynamicSystemWebCompilation>());
        services.Services.AddSingleton<IHttpHandlerCollection>(ctx => ctx.GetRequiredService<DynamicSystemWebCompilation>());

        return services;
    }

    private static void AddWebFormsCompilationCore(this IServiceCollection services, Action<PageCompilationOptions> configure)
    {
        services.AddOptions<PageCompilationOptions>()
            .Configure(options =>
            {
                options.AddParser<PageParser>(".aspx");
                options.AddParser<MasterPageParser>(".Master");

                //TODO https://github.com/twsouthwick/systemweb-adapters-ui/issues/19 , keeping the code to tackle in next CR
                //options.AddParser<UserControlParser>(".ascx");
            })
            .Configure(configure);

        services.AddOptions<PagesSection>()
            .Configure<IOptions<PageCompilationOptions>>((options, compilation) =>
            {
                options.EnableSessionState = System.Web.Configuration.PagesEnableSessionState.True;
            })
            .Configure<IMetadataProvider>((options, metadata) =>
            {
                foreach (var control in metadata.ControlAssemblies)
                {
                    foreach (var tag in control.GetCustomAttributes<TagPrefixAttribute>())
                    {
                        options.DefaultTagNamespaceRegisterEntries.Add(new(tag.TagPrefix, tag.NamespaceName, control.FullName));
                    }
                }
            });
    }
}
