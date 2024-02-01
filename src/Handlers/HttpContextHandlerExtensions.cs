// MIT License.

using System.Runtime.CompilerServices;
using System.Web.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

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

    public static IEndpointConventionBuilder MapHttpHandler<T>(this IEndpointRouteBuilder endpoints, string path)
       where T : IHttpHandler
    {
        if (endpoints.DataSources.OfType<HttpHandlerManagerBuilder>().FirstOrDefault() is not { } existing)
        {
            existing = new HttpHandlerManagerBuilder();
            endpoints.DataSources.Add(existing);
        }

        existing.Add<T>(path);

        return existing;
    }

    private sealed class HttpHandlerManagerBuilder() : HttpHandlerEndpointConventionBuilder(new HttpHandlerManager())
    {
        public void Add<T>(string path) => ((HttpHandlerManager)Manager).Add(HandlerEndpointBuilder.Create(path, typeof(T)));
    }

    private sealed class HttpHandlerManager : IHttpHandlerManager
    {
        private readonly List<EndpointBuilder> _handlers = new();

        public IEnumerable<EndpointBuilder> GetBuilders() => _handlers;

        public IChangeToken GetChangeToken() => NullChangeToken.Singleton;

        public void Add(EndpointBuilder builder) => _handlers.Add(builder);
    }
}

