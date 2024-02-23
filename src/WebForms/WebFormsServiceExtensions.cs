// MIT License.

using System.Web;
using System.Web.Compilation;
using System.Web.UI;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using WebForms;

[assembly: TagPrefix("System.Web.UI", "asp")]
[assembly: TagPrefix("System.Web.UI.WebControls", "asp")]

namespace Microsoft.AspNetCore.Builder;

public static class WebFormsServiceExtensions
{
    public static IWebFormsBuilder AddWebForms(this ISystemWebAdapterBuilder builder, Action<WebFormsOptions> configure = null)
    {
        var optionsBuilder = builder.Services.AddOptions<WebFormsOptions>()
            .Configure<IHostEnvironment>((options, env) => options.WebFormsFileProvider = env.ContentRootFileProvider);

        if (configure is not null)
        {
            optionsBuilder.Configure(configure);
        }

        builder.AddHttpHandlers();
        builder.AddRouting();
        builder.AddVirtualPathProvider();

        return new Builder(builder)
            .AddDefaultExpressionBuilders();
    }

    public static IWebFormsBuilder AddWebForms(this IServiceCollection builder, Action<WebFormsOptions> configure = null)
        => builder
            .AddSystemWebAdapters()
            .AddWrappedAspNetCoreSession()
            .AddWebForms(configure);

    public static IWebFormsBuilder AddDefaultExpressionBuilders(this IWebFormsBuilder builder) => builder
        .AddExpressionBuilder<RouteUrlExpressionBuilder>("RouteUrl");

    private static IWebFormsBuilder AddExpressionBuilder<T>(this IWebFormsBuilder builder, string name)
        where T : ExpressionBuilder, new()
    {
        builder.Services.TryAddSingleton<ExpressionBuilderCollection>();
        var factory = ActivatorUtilities.CreateFactory(typeof(T), []);

        builder.Services.AddOptions<ExpressionBuilderCollection.ExpressionOption>()
            .Configure<IServiceProvider>((options, sp) => options.Add(name, () => (ExpressionBuilder)factory(sp, null)));

        return builder;
    }

    private record Builder(ISystemWebAdapterBuilder SystemWebAdapterBuilder) : IWebFormsBuilder
    {
        public IServiceCollection Services => SystemWebAdapterBuilder.Services;
    }
}
