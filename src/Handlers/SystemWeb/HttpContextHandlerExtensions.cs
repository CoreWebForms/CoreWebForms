// MIT License.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.SystemWebAdapters.Features;
using Microsoft.AspNetCore.SystemWebAdapters.HttpHandlers;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace System.Web;

public static class HttpContextHandlerExtensions
{
    public static void SetHandler(this HttpContext context, IHttpHandler handler)
        => ((HttpContextCore)context).Features.GetRequired<IHttpHandlerFeature>().Current = handler;

    public static IHttpHandler? GetHandler(this HttpContext context)
        => ((HttpContextCore)context).Features.GetRequired<IHttpHandlerFeature>().Current;

    public static IEndpointConventionBuilder MapHttpHandler<T>(this IEndpointRouteBuilder endpoints, string path)
       where T : IHttpHandler
        => endpoints.MapHttpHandler(path, typeof(T));

    public static IEndpointConventionBuilder MapHttpHandler(this IEndpointRouteBuilder endpoints, string path, Type type)
    {
        if (endpoints.DataSources.OfType<HttpHandlerManagerBuilder>().FirstOrDefault() is not { } existing)
        {
            existing = new HttpHandlerManagerBuilder();
            endpoints.DataSources.Add(existing);
        }

        existing.Add(path, type);

        return existing;
    }

    private sealed class HttpHandlerManagerBuilder() : HttpHandlerEndpointConventionBuilder(new HttpHandlerManager())
    {
        public void Add(string path, Type type) => ((HttpHandlerManager)Manager).Add(HandlerEndpointBuilder.Create(path, type));
    }

    private sealed class HttpHandlerManager : IHttpHandlerManager
    {
        private readonly List<EndpointBuilder> _handlers = new();

        public IEnumerable<EndpointBuilder> GetBuilders() => _handlers;

        public IChangeToken GetChangeToken() => NullChangeToken.Singleton;

        public void Add(EndpointBuilder builder) => _handlers.Add(builder);
    }
}

