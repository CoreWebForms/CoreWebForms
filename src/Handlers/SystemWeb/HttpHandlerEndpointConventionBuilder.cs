// MIT License.

using System.Collections.Immutable;
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
    private static readonly ImmutableList<object> _metadata = new object[]
    {
        new BufferResponseStreamAttribute(),
        new PreBufferRequestStreamAttribute(),
        new SetThreadCurrentPrincipalAttribute(),
    }.ToImmutableList();

    private static readonly ImmutableList<object> _metadataReadonlySession = _metadata.Add(new SessionAttribute { SessionBehavior = SessionStateBehavior.ReadOnly });
    private static readonly ImmutableList<object> _metadataSession = _metadata.Add(new SessionAttribute { SessionBehavior = SessionStateBehavior.Required });

    private readonly AdditionalManager _additional;
    private readonly IHttpHandlerManager[] _managers;
    private List<Action<EndpointBuilder>> _conventions = [];

    internal HttpHandlerEndpointConventionBuilder(IEnumerable<IHttpHandlerManager> managers)
    {
        _additional = new AdditionalManager();
        _managers = [.. managers, _additional];
    }

    public void Add(string path, Type type) => _additional.Add(path, type);

    public override IReadOnlyList<Endpoint> Endpoints
    {
        get
        {
            var endpoints = new List<Endpoint>();

            foreach (var manager in _managers)
            {
                foreach (var metadata in manager.GetHandlerMetadata())
                {
                    var pattern = RoutePatternFactory.Parse(metadata.Route);
                    var builder = new RouteEndpointBuilder(DefaultHandler, pattern, 0);

                    AddHttpHandlerMetadata(builder, metadata);

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
            }

            return endpoints;
        }
    }

    internal static void AddHttpHandlerMetadata(EndpointBuilder builder, IHttpHandlerMetadata metadata)
    {
        var intrinsicMetadata = metadata.Behavior switch
        {
            SessionStateBehavior.ReadOnly => _metadataReadonlySession,
            SessionStateBehavior.Required => _metadataSession,
            _ => _metadata
        };

        builder.Metadata.Add(metadata);

        foreach (var intrinsic in intrinsicMetadata)
        {
            builder.Metadata.Add(intrinsic);
        }
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

    private sealed class AdditionalManager : IHttpHandlerManager
    {
        private readonly List<IHttpHandlerMetadata> _endpoints = new();

        public void Add(string path, Type type) => _endpoints.Add(HandlerMetadata.Create(path, type));

        public IEnumerable<IHttpHandlerMetadata> GetHandlerMetadata() => _endpoints;

        // We can use a null change token because we only expect this to be used at the time of registration
        public IChangeToken GetChangeToken() => NullChangeToken.Singleton;
    }
}
