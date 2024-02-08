// MIT License.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.SystemWebAdapters.HttpHandlers;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

using RouteCollection = System.Web.Routing.RouteCollection;

namespace Microsoft.Extensions.DependencyInjection;

public static class RoutingServiceExtensions
{
    public static ISystemWebAdapterBuilder AddRouting(this ISystemWebAdapterBuilder builder, string defaultPage)
        => builder.AddRouting(routes => routes.MapPageRoute("DefaultPage", "/", defaultPage));

    public static ISystemWebAdapterBuilder AddRouting(this ISystemWebAdapterBuilder builder, Action<RouteCollection> routeConfiguration)
    {
        builder.AddRouting();
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IStartupFilter, RouteStartupFilter>());

        builder.Services.AddOptions<RouteCollectionOption>()
            .Configure(options => options.Configure += routeConfiguration);

        return builder;
    }

    public static ISystemWebAdapterBuilder AddRouting(this ISystemWebAdapterBuilder builder)
    {
        if (builder.Services.Any(d => d.ServiceType == typeof(RouteCollection)))
        {
            return builder;
        }

        builder.Services.AddSingleton(ctx => new RouteCollection(ctx.GetRequiredService<TemplateBinderFactory>(), ctx.GetRequiredService<IOptions<RouteOptions>>()));
        builder.Services.AddSingleton<IHttpHandlerCollection>(ctx => ctx.GetRequiredService<RouteCollection>());

        return builder;
    }

    private sealed class RouteCollectionOption
    {
        public Action<RouteCollection>? Configure { get; set; }
    }

    private sealed class RouteStartupFilter(IOptions<RouteCollectionOption> routes) : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
            => builder =>
            {
                routes.Value.Configure?.Invoke(builder.ApplicationServices.GetRequiredService<RouteCollection>());

                next(builder);
            };
    }
}
