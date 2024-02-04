// MIT License.

using System.Web.Compilation;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WebForms;

namespace Microsoft.AspNetCore.Builder;

public static class WebFormsServiceExtensions
{
    public static IWebFormsBuilder AddWebForms(this ISystemWebAdapterBuilder builder)
    {
        builder.AddHttpHandlers();
        builder.AddRouting();

        return new Builder(builder)
            .AddDefaultExpressionBuilders();
    }

    public static IWebFormsBuilder AddWebForms(this IServiceCollection builder)
        => builder
            .AddSystemWebAdapters()
            .AddWrappedAspNetCoreSession()
            .AddWebForms();

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
