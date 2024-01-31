// MIT License.

using System.Runtime.CompilerServices;
using System.Web.Routing;
using Microsoft.AspNetCore.Http;

namespace System.Web;

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

    private static Endpoint Create(IHttpHandler handler) => RouteItem.Create(handler).GetBuilder().Build();
}
