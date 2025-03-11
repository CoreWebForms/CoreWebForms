// MIT License.

using System.Runtime.CompilerServices;
using System.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.SystemWebAdapters.HttpHandlers;

internal sealed class HttpHandlerEndpointFactory(ILogger<HttpHandlerEndpointFactory> logger, IServiceProvider services) : IHttpHandlerEndpointFactory
{
    // Used to ensure we only create a single endpoint instance for a given handler. However,
    // we don't have a way to track when a handler ceases to exist, so we use the
    // ConditionalWeakTable<,> to track that for us to cache handlers while they exist.
    // (i.e. they may be create on demand, so would end up not being cached long, but they also
    // be created once, cached somewhere, and reused multiple times)
    private readonly ConditionalWeakTable<IHttpHandler, Endpoint> _table = new();

    public RequestDelegate DefaultHandler { get; } = BuildDefaultHandler(services);

    Endpoint IHttpHandlerEndpointFactory.Create(IHttpHandler handler)
    {
        return handler.IsReusable ? UseCache(handler) : Create(handler);
    }

    private Endpoint UseCache(IHttpHandler handler)
    {
        if (_table.TryGetValue(handler, out var existing))
        {
            logger.LogTrace("Using cached endpoint for {Handler}", handler.GetType());
            return existing;
        }

        var newEndpoint = Create(handler);

        _table.Add(handler, newEndpoint);

        return newEndpoint;
    }

    private Endpoint Create(IHttpHandler handler)
    {
        logger.LogTrace("Creating endpoint for {Handler}", handler.GetType());

        var builder = new NonRouteEndpointBuilder(DefaultHandler);
        var metadata = HandlerMetadata.Create("/", handler);

        builder.AddHandler(metadata);

        return builder.Build();
    }

    private static RequestDelegate BuildDefaultHandler(IServiceProvider services)
    {
        var builder = new ApplicationBuilder(services);

        builder.EnsureRequestEndThrows();
        builder.Run(context =>
        {
            if (context.AsSystemWeb().CurrentHandler is { } handler)
            {
                return handler.RunHandlerAsync(context).AsTask();
            }

            context.Response.StatusCode = 500;
            return context.Response.WriteAsync("Invalid handler");
        });

        return builder.Build();
    }

    private sealed class NonRouteEndpointBuilder: EndpointBuilder
    {
        public NonRouteEndpointBuilder(RequestDelegate requestDelegate)
        {
            RequestDelegate = requestDelegate;
        }

        public override Endpoint Build()
        {
            if (RequestDelegate is null)
            {
                throw new InvalidOperationException($"{nameof(RequestDelegate)} must be specified to construct a {nameof(RouteEndpoint)}.");
            }

            return new Endpoint(RequestDelegate, CreateMetadataCollection(Metadata), DisplayName);
        }

        private static EndpointMetadataCollection CreateMetadataCollection(IList<object> metadata)
        {
            return new EndpointMetadataCollection(metadata);
        }
    }
}
