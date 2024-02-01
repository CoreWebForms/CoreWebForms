// MIT License.

using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.SystemWebAdapters.Features;

namespace System.Web.Routing;

public static class RoutingExtensions
{
    public static RequestContext GetRequestContext(this HttpContext httpContext)
        => httpContext.AsAspNetCore().Features.GetRequiredFeature<IRouteDataFeature>().RequestContext;

    public static RouteData GetRouteData(this HttpRequest request)
        => request.AsAspNetCore().HttpContext.Features.GetRequiredFeature<IRouteDataFeature>().RouteData;
}

