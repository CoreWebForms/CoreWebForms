// MIT License.

using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.SystemWebAdapters.Features;

namespace System.Web.Routing;

public static class RoutingExtensions
{
    public static RequestContext GetRequestContext(this HttpContext httpContext)
        => httpContext.Request.GetOrCreateRouteDataFeature().RequestContext;

    public static RouteData GetRouteData(this HttpRequest request)
        => request.GetOrCreateRouteDataFeature().RouteData;

    private static IRouteDataFeature GetOrCreateRouteDataFeature(this HttpRequest request)
    {
        var features = request.AsAspNetCore().HttpContext.Features;

        if (features.Get<IRouteDataFeature>() is { } feature)
        {
            return feature;
        }

        feature = new RouteDataFeature(request.AsAspNetCore().HttpContext);
        features.Set(feature);

        return feature;
    }
}

