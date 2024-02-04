// MIT License.

using System.Web.SessionState;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.SystemWebAdapters.Features;
using Microsoft.AspNetCore.SystemWebAdapters.HttpHandlers;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace System.Web;

internal sealed class HttpHandlerEndpointConventionBuilder : EndpointDataSource, IEndpointConventionBuilder
{
    private readonly AdditionalManager _additional;
    private readonly IHttpHandlerCollection[] _managers;
    private readonly HandlerMetadataProvider _metadataProvider;
    private List<Action<EndpointBuilder>> _conventions = [];

    public HttpHandlerEndpointConventionBuilder(IEnumerable<IHttpHandlerCollection> managers, HandlerMetadataProvider metadataProvider)
    {
        _additional = new AdditionalManager();
        _managers = [.. managers, _additional];
        _metadataProvider = metadataProvider;
    }

    public void Add(string path, Type type) => _additional.Add(path, type);

    public override IReadOnlyList<Endpoint> Endpoints
    {
        get
        {
            var endpoints = new List<Endpoint>();

            foreach (var metadata in CollectMetadata())
            {
                var pattern = RoutePatternFactory.Parse(metadata.Route);
                var builder = new RouteEndpointBuilder(DefaultHandler, pattern, 0);

                _metadataProvider.Add(builder, metadata);

                foreach (var convention in _conventions)
                {
                    convention(builder);
                }

#if NET7_0_OR_GREATER
                if (builder.FilterFactories.Count > 0)
                {
                    throw new NotSupportedException("Filter factories are not supported for handlers");
                }
#endif

                endpoints.Add(builder.Build());
            }

            return endpoints;
        }
    }

    private IEnumerable<IHttpHandlerMetadata> CollectMetadata()
    {
        var metadataCollection = new Dictionary<string, IHttpHandlerMetadata>();
        var mappedRoutes = new List<NamedHttpHandlerRoute>();

        foreach (var manager in _managers)
        {
            mappedRoutes.AddRange(manager.NamedRoutes);

            foreach (var metadata in manager.GetHandlerMetadata())
            {
                metadataCollection.Add(metadata.Route, metadata);
            }
        }

        foreach (var mappedRoute in mappedRoutes)
        {
            metadataCollection.Add(mappedRoute.Route, new MappedHandlerMetadata(mappedRoute.Route, metadataCollection[mappedRoute.Path]));
        }

        return metadataCollection.Values;
    }

    private sealed class MappedHandlerMetadata(string route, IHttpHandlerMetadata metadata) : IHttpHandlerMetadata
    {
        public SessionStateBehavior Behavior => metadata.Behavior;

        public string Route => route;

        public ValueTask<IHttpHandler> Create(HttpContextCore context) => metadata.Create(context);
    }

    public void Add(Action<EndpointBuilder> convention)
        => (_conventions ??= []).Add(convention);

    public override IChangeToken GetChangeToken() => new CompositeChangeToken(_managers.Select(m => m.GetChangeToken()).ToArray());

    private static Task DefaultHandler(HttpContextCore context)
    {
        if (context.Features.GetRequiredFeature<IHttpHandlerFeature>().Current is { } handler)
        {
            return handler.RunHandlerAsync(context).AsTask();
        }

        context.Response.StatusCode = 500;
        return context.Response.WriteAsync("Invalid handler");
    }

    private sealed class AdditionalManager : IHttpHandlerCollection
    {
        private readonly List<IHttpHandlerMetadata> _endpoints = new();

        public IEnumerable<NamedHttpHandlerRoute> NamedRoutes => [];

        public void Add(string path, Type type) => _endpoints.Add(HandlerMetadata.Create(path, type));

        public IEnumerable<IHttpHandlerMetadata> GetHandlerMetadata() => _endpoints;

        // We can use a null change token because we only expect this to be used at the time of registration
        public IChangeToken GetChangeToken() => NullChangeToken.Singleton;
    }
}
