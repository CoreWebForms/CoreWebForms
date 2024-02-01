// MIT License.

using System.Web;
using System.Web.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Extensions.DependencyInjection;
using RouteData = System.Web.Routing.RouteData;

namespace WebForms.Routing;

public static class RoutingHttpContextExtensions
{
    public static ISystemWebAdapterBuilder AddRouting(this ISystemWebAdapterBuilder builder)
    {
        builder.Services.AddSingleton(new System.Web.Routing.RouteCollection());
        return builder;
    }

    public static RequestContext GetRequestContext(this HttpContext httpContext)
    {
        if (httpContext.AsAspNetCore().Features.Get<RequestContext>() is { } existing)
        {
            return existing;
        }

        var routeData = GetRouteData(httpContext.Request);
        var requestContext = new RequestContext(new HttpContextWrapper(httpContext), routeData);
        httpContext.AsAspNetCore().Features.Set<RequestContext>(requestContext);

        return requestContext;
    }

    public static RouteData GetRouteData(this HttpRequest request)
    {
        var coreRequest = request.AsAspNetCore();

        if (coreRequest.HttpContext.Features.Get<RouteData>() is { } existing)
        {
            return existing;
        }

        var data = new RouteData(coreRequest.HttpContext.GetRouteData());

        coreRequest.HttpContext.Features.Set<RouteData>(data);

        return data;
    }
}
