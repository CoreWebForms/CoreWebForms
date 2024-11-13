// MIT License.

using System.Runtime.CompilerServices;
using System.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNetCore.SystemWebAdapters.HttpHandlers;

internal sealed class HttpHandlerEndpointFactory : IHttpHandlerEndpointFactory
{
    // Used to ensure we only create a single endpoint instance for a given handler. However,
    // we don't have a way to track when a handler ceases to exist, so we use the
    // ConditionalWeakTable<,> to track that for us to cache handlers while they exist.
    // (i.e. they may be create on demand, so would end up not being cached long, but they also
    // be created once, cached somewhere, and reused multiple times)
    private readonly ConditionalWeakTable<IHttpHandler, Endpoint> _table = new();

    Endpoint IHttpHandlerEndpointFactory.Create(IHttpHandler handler)
        => handler.IsReusable ? UseCache(handler) : Create(handler);

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

    private static Endpoint Create(IHttpHandler handler)
    {
        var builder = new NonRouteEndpointBuilder { RequestDelegate = context =>
            {
                handler.ProcessRequest(context.AsSystemWeb());
                return Task.CompletedTask;
            }
        };
        var metadata = HandlerMetadata.Create("/", handler);

        builder.AddHandler(metadata);

        return builder.Build();
    }

    internal sealed class NonRouteEndpointBuilder : EndpointBuilder
    {
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
