// MIT License.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.SystemWebAdapters.HttpHandlers;
using Microsoft.Extensions.Options;

using RouteCollection = System.Web.Routing.RouteCollection;

namespace Microsoft.Extensions.DependencyInjection;

public static class RoutingServiceExtensions
{
    public static ISystemWebAdapterBuilder AddRouting(this ISystemWebAdapterBuilder builder)
    {
        builder.Services.AddSingleton(ctx => new RouteCollection(ctx.GetRequiredService<TemplateBinderFactory>(), ctx.GetRequiredService<IOptions<RouteOptions>>()));
        builder.Services.AddSingleton<IHttpHandlerCollection>(ctx => ctx.GetRequiredService<RouteCollection>());

        return builder;
    }
}
