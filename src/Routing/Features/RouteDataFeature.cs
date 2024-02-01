// MIT License.

using System.Web;
using System.Web.Routing;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNetCore.SystemWebAdapters.Features;

internal sealed class RouteDataFeature(HttpContextCore context) : IRouteDataFeature, IRoutingFeature
{
    private RequestContext? _requestContext;
    private System.Web.Routing.RouteData? _routeData;

    public RequestContext RequestContext
    {
        get => _requestContext ??= new RequestContext(new HttpContextWrapper(context), RouteData);
    }

    public System.Web.Routing.RouteData RouteData
    {
        get => _routeData ??= new(context.GetRouteData());
        set => _routeData = value;
    }

    Routing.RouteData? IRoutingFeature.RouteData
    {
        get => RouteData?.AsAspNetCore();
        set
        {
            _requestContext = null;
            _routeData = value is null ? null : new(value);
        }
    }
}
