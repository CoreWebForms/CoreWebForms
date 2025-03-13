// MIT License.

using System.Web.SessionState;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.AspNetCore.SystemWebAdapters.HttpHandlers;
using Microsoft.Extensions.Primitives;

namespace System.Web;

internal sealed class HttpHandlerEndpointConventionBuilder(
    IEnumerable<IHttpHandlerCollection> managers,
    IHttpHandlerEndpointFactory factory) : EndpointDataSource, IEndpointConventionBuilder
{
    private readonly IHttpHandlerCollection[] _managers = [.. managers];
    private List<Action<EndpointBuilder>> _conventions = [];

    public override IReadOnlyList<Endpoint> Endpoints
    {
        get
        {
            var endpoints = new List<Endpoint>();

            foreach (var (route, metadataCollection) in CollectMetadata())
            {
                var pattern = RoutePatternFactory.Parse(route);
                var builder = factory.CreateBuilder(pattern);

                builder.AddHandler(metadataCollection);

                foreach (var convention in _conventions)
                {
                    convention(builder);
                }

                if (builder.FilterFactories.Count > 0)
                {
                    throw new NotSupportedException("Filter factories are not supported for handlers");
                }

                endpoints.Add(builder.Build());
            }

            return endpoints;
        }
    }

    private Dictionary<string, List<object>> CollectMetadata()
    {
        var metadataCollection = new Dictionary<string, List<object>>();
        var mappedRoutes = new List<NamedHttpHandlerRoute>();

        foreach (var manager in _managers)
        {
            mappedRoutes.AddRange(manager.NamedRoutes);

            foreach (var metadata in manager.GetHandlerMetadata())
            {
                metadataCollection.Add(metadata.Route, [metadata]);
            }
        }

        foreach (var mappedRoute in mappedRoutes)
        {
            // TODO should we log if we can't find it? It may be a race condition where the compilation hasn't found it yet, so it could be an unnecessary warning
            if (metadataCollection.TryGetValue(mappedRoute.Path, out var fromCollection) && fromCollection is [IHttpHandlerMetadata handler, ..])
            {
                metadataCollection.Add(mappedRoute.Route, [.. fromCollection, new MappedHandlerMetadata(mappedRoute.Route, handler)]);
            }
        }

        return metadataCollection;
    }

    public void Add(Action<EndpointBuilder> convention)
        => (_conventions ??= []).Add(convention);

    public override IChangeToken GetChangeToken() => new CompositeChangeToken(_managers.Select(m => m.GetChangeToken()).ToArray());

    private sealed class MappedHandlerMetadata(string route, IHttpHandlerMetadata metadata) : IHttpHandlerMetadata
    {
        public SessionStateBehavior Behavior => metadata.Behavior;

        public string Route => route;

        public IHttpHandler Create(HttpContextCore context) => metadata.Create(context);
    }
}
