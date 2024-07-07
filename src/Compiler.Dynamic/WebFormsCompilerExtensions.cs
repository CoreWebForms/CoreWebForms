// MIT License.

using System.Collections.Immutable;
using System.ComponentModel.Design;
using System.Globalization;
using System.Reflection;
using System.Web;
using System.Web.UI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SystemWebAdapters.HttpHandlers;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Hosting;
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
        services.Services.AddTransient<IStartupFilter, DynamicSystemWebCompilationStartup>();
        services.Services.AddTransient<IStartupFilter, CompilationFeatureStartupFilter>();
        services.Services.AddSingleton<IHttpHandlerCollection>(ctx => ctx.GetRequiredService<DynamicSystemWebCompilation>());

        return services;
    }

    private static void AddWebFormsCompilationCore(this IServiceCollection services, Action<PageCompilationOptions> configure)
    {
        services.AddSingleton<IWebFormsCompiler, SystemWebCompilation>();
        services.AddOptions<PageCompilationOptions>()
            .Configure<IOptions<WebFormsOptions>>((options, webFormsOptions) =>
            {
                options.WebFormsFileProvider = webFormsOptions.Value.WebFormsFileProvider;
                options.AddParser<PageDependencyParser>(".aspx");
                options.AddParser<MasterPageDependencyParser>(".Master");
                options.AddParser<UserControlDependencyParser>(".ascx");
            })
            .Configure<IWebHostEnvironment>((options, env) =>
            {
                options.IsDebug = env.IsDevelopment();
            })
            .Configure(configure);

        services.AddOptions<PagesSection>()
            .Configure<IOptions<PageCompilationOptions>>((options, compilation) =>
            {
                options.EnableSessionState = System.Web.Configuration.PagesEnableSessionState.True;

                foreach (var ns in compilation.Value.Namespaces)
                {
                    options.Namespaces.NamespaceEntries ??= new();
                    options.Namespaces.NamespaceEntries.Add(ns, new NamespaceEntry { Namespace = ns });
                }

                foreach (var entry in compilation.Value.Entries)
                {
                    options.DefaultTagNamespaceRegisterEntries.Add(entry);
                }
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

    internal static IEnumerable<RoslynError> ConvertToErrors(this ImmutableArray<Diagnostic> diagnostics)
    {
        foreach (var d in diagnostics)
        {
            yield return new RoslynError()
            {
                Id = d.Id,
                Message = d.GetMessage(CultureInfo.CurrentCulture),
                Severity = d.Severity,
                Location = d.Location.ToString(),
            };
        }
    }

    private sealed class CompilationFeatureStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
            => builder =>
            {
                builder.UseMiddleware<SetCompilationFeatureMiddleware>();
                next(builder);
            };
    }
}
