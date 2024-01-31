// MIT License.

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

    public static ISystemWebAdapterBuilder AddHandlers(this ISystemWebAdapterBuilder builder, Action<IHttpHandlerBuilder> configure)
    {
        var manager = new HttpHandlerManager();

        configure(manager);

        builder.Services.AddSingleton<IHttpHandlerManager>(manager);

        return builder;
    }

    private sealed class HttpHandlerManager : IHttpHandlerManager, IHttpHandlerBuilder
    {
        private readonly List<(string, IHttpHandler)> _handlers = new();

        public IEnumerable<EndpointBuilder> GetBuilders()
        {
            foreach (var (path, handler) in _handlers)
            {
                yield return HandlerEndpointBuilder.Create(path, handler);
            }
        }

        public IChangeToken GetChangeToken() => NullChangeToken.Singleton;

        void IHttpHandlerBuilder.Add(string path, IHttpHandler handler)
            => _handlers.Add((path, handler));
    }
}

