// MIT License.

using System.Collections.Immutable;
using System.ComponentModel.Design;
using System.Globalization;
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
    private static readonly HashSet<string> _diagnosticsToSkip = [
        "CS1701", // Assembly unification error not applicable on .NET Core
        "CS1702", // Assembly unification error not applicable on .NET Core
        ];

    public static IWebFormsBuilder AddPersistentWebFormsCompilation(this IWebFormsBuilder builder, IEnumerable<string> paths)
    {
        builder.Services.AddSingleton<DynamicControlCollection>();
        builder.Services.AddSingleton<StaticControlCollection>();
        builder.Services.AddSingleton<ITypeResolutionService>(ctx => ctx.GetRequiredService<StaticControlCollection>());
        builder.Services.AddSingleton<IMetadataProvider>(ctx => ctx.GetRequiredService<StaticControlCollection>());
        builder.Services.AddWebFormsCompilationCore(options =>
        {
            options.RegisterAdditionalReferencePaths(paths);
        });

        return builder;
    }

    public static IWebFormsBuilder AddDynamicPages(this IWebFormsBuilder services)
        => services.AddDynamicPages(_ => { });

    public static IWebFormsBuilder AddDynamicPages(this IWebFormsBuilder services, Action<PageCompilationOptions> configure)
    {
        services.Services.AddSingleton<DynamicControlCollection>();
        services.Services.AddSingleton<ITypeResolutionService>(ctx => ctx.GetRequiredService<DynamicControlCollection>());
        services.Services.AddSingleton<IMetadataProvider>(ctx => ctx.GetRequiredService<DynamicControlCollection>());
        services.Services.AddOptions<PageCompilationOptions>()
            .Configure(options =>
            {
                options.RegisterAdditionalReferencePaths(Directory.EnumerateFiles(AppContext.BaseDirectory, "*.dll"));
            });
        services.Services.AddWebFormsCompilationCore(configure);
        services.Services.AddHostedService<WebFormsCompilationService>();
        services.Services.AddSingleton<DynamicSystemWebCompilation>();
        services.Services.AddTransient<IStartupFilter, DynamicSystemWebCompilationStartup>();
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
            })
            .Configure<IMetadataProvider>((options, metadata) =>
            {
                options.DefaultTagNamespaceRegisterEntries = metadata.TagRegistrations;
            });
    }

    internal static IEnumerable<RoslynError> ConvertToErrors(this ImmutableArray<Diagnostic> diagnostics)
    {
        foreach (var d in diagnostics)
        {
            if (!_diagnosticsToSkip.Contains(d.Id))
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
    }
}
