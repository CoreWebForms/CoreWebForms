// MIT License.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

public static class RoutingServiceExtensions
{
    public static ISystemWebAdapterBuilder AddRouting(this ISystemWebAdapterBuilder builder)
    {
        builder.Services.AddSingleton(ctx => new System.Web.Routing.RouteCollection(ctx.GetRequiredService<TemplateBinderFactory>(), ctx.GetRequiredService<IOptions<RouteOptions>>()));
        builder.Services.AddTransient<IStartupFilter, RoutingStartupFilter>();

        return builder;
    }

    private sealed class RoutingStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
            => builder =>
            {
                builder.UseMiddleware<SystemWebRoutingMiddleware>();
                next(builder);
            };
    }
}
