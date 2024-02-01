// MIT License.

using System.Web.Routing;
using Microsoft.AspNetCore.SystemWebAdapters.Features;

namespace Microsoft.Extensions.DependencyInjection;

internal sealed class SystemWebRoutingMiddleware(AspNetCore.Http.RequestDelegate next, RouteCollection routes)
{
    public Task InvokeAsync(HttpContextCore context)
    {
        var feature = new RouteDataFeature(context);
        context.Features.Set<IRouteDataFeature>(feature);

        if (routes.TryGetMapped(context.Request.Path, out var newPath, out var kv))
        {
            context.Request.Path = newPath;
            feature.RouteData = new(new(kv));
        }

        return next(context);
    }
}

