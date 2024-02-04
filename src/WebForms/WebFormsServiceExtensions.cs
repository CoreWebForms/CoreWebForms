// MIT License.

using System.Web.Compilation;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Extensions.DependencyInjection;
using WebForms;

namespace Microsoft.AspNetCore.Builder;

public static class WebFormsServiceExtensions
{
    public static IWebFormsBuilder AddWebForms(this ISystemWebAdapterBuilder builder)
    {
        builder.AddHttpHandlers();
        builder.AddRouting();

        return new Builder(builder);
    }

    public static IWebFormsBuilder AddWebForms(this IServiceCollection builder)
        => builder
            .AddSystemWebAdapters()
            .AddWrappedAspNetCoreSession()
            .AddWebForms()
            .AddDefaultExpressionBuilders();

    public static IWebFormsBuilder AddDefaultExpressionBuilders(this IWebFormsBuilder builder) => builder
        .AddExpressionBuilder<RouteUrlExpressionBuilder>("RouteUrl");

    private static IWebFormsBuilder AddExpressionBuilder<T>(this IWebFormsBuilder builder, string name)
        where T : ExpressionBuilder
    {
        var factory = ActivatorUtilities.CreateFactory(typeof(T), []);

        builder.Services.AddOptions<ExpressionBuilderOption>(name)
            .Configure<IServiceProvider>((options, provider) => options.Factory = () => (ExpressionBuilder)factory(provider, null));

        return builder;
    }

    private record Builder(ISystemWebAdapterBuilder SystemWebAdapterBuilder) : IWebFormsBuilder
    {
        public IServiceCollection Services => SystemWebAdapterBuilder.Services;
    }
}
