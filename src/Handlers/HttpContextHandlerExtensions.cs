// MIT License.

using System.Web.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SystemWebAdapters;

using RouteData = System.Web.Routing.RouteData;

namespace System.Web;

public static class HttpContextHandlerExtensions
{
    public static void SetHandler(this HttpContext context, IHttpHandler handler)
        => ((HttpContextCore)context).Features.GetRequired<IHttpHandlerFeature>().Current = handler;

    public static IHttpHandler? GetHandler(this HttpContext context)
        => ((HttpContextCore)context).Features.GetRequired<IHttpHandlerFeature>().Current;

    public static RouteData GetRouteData(this HttpRequest request)
    {
        var coreRequest = ((HttpRequestCore)request);

        if (coreRequest.HttpContext.Features.Get<RouteData>() is { } existing)
        {
            return existing;
        }

        var data = new RouteData(coreRequest.HttpContext.GetRouteData());

        coreRequest.HttpContext.Features.Set<RouteData>(data);

        return data;
    }

    public static RequestContext GetRequestContext(this HttpContextCore httpContext)
    {
        if (httpContext.Features.Get<RequestContext>() is { } existing)
        {
            return existing;
        }

        var routeData = GetRouteData(httpContext.Request);
        var requestContext = new RequestContext(new HttpContextWrapper(httpContext), routeData);
        httpContext.Features.Set<RequestContext>(requestContext);

        return requestContext;
    }
}

