// MIT License.

using System.Runtime.CompilerServices;
using System.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.AspNetCore.SystemWebAdapters.Features;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.SystemWebAdapters.HttpHandlers;

internal sealed class HttpHandlerEndpointFactory(ILogger<HttpHandlerEndpointFactory> logger, IServiceProvider services) : IHttpHandlerEndpointFactory
{
    // Used to ensure we only create a single endpoint instance for a given handler. However,
    // we don't have a way to track when a handler ceases to exist, so we use the
    // ConditionalWeakTable<,> to track that for us to cache handlers while they exist.
    // (i.e. they may be create on demand, so would end up not being cached long, but they also
    // be created once, cached somewhere, and reused multiple times)
    private readonly ConditionalWeakTable<IHttpHandler, Endpoint> _table = [];

    public RequestDelegate DefaultHandler { get; } = BuildDefaultHandler(services);

    Endpoint IHttpHandlerEndpointFactory.Create(IHttpHandler handler)
        => handler.IsReusable ? UseCache(handler) : Create(handler);

    EndpointBuilder IHttpHandlerEndpointFactory.CreateBuilder(RoutePattern pattern)
        => new RouteEndpointBuilder(DefaultHandler, pattern, 0);

    private Endpoint UseCache(IHttpHandler handler)
    {
        if (_table.TryGetValue(handler, out var existing))
        {
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
            if (context.Features.GetRequiredFeature<IHttpHandlerFeature>().Current is { } handler)
            {
                if (handler is HttpTaskAsyncHandler task)
                {
                    return task.ProcessRequestAsync(context);
                }
                else if (handler is IHttpAsyncHandler asyncHandler)
                {
                    return Task.Factory.FromAsync((cb, state) => asyncHandler.BeginProcessRequest(context, cb, state), asyncHandler.EndProcessRequest, null);
                }
                else
                {
                    handler.ProcessRequest(context);
                    return Task.CompletedTask;
                }
            }

            context.Response.StatusCode = 500;
            return context.Response.WriteAsync("Invalid handler");
        });

        return builder.Build();
    }

    private sealed class NonRouteEndpointBuilder(RequestDelegate requestDelegate) : EndpointBuilder
    {
        public override Endpoint Build() => new(requestDelegate, new(Metadata), DisplayName);
    }
}
