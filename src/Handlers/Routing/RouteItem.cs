// MIT License.

using System.Collections.Immutable;
using System.Web.SessionState;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Extensions.DependencyInjection;

namespace System.Web.Routing;

internal sealed record RouteItem(IHttpHandlerMetadata Handler, SessionStateBehavior SessionState)
{
    private static readonly ImmutableList<object> _metadata = new object[]
    {
        new BufferResponseStreamAttribute(),
        new PreBufferRequestStreamAttribute(),
        new SetThreadCurrentPrincipalAttribute(),
        //new SingleThreadedRequestAttribute(),
    }.ToImmutableList();

    private static readonly ImmutableList<object> _metadataReadonlySession = _metadata.Add(new SessionAttribute { SessionBehavior = SessionStateBehavior.ReadOnly });
    private static readonly ImmutableList<object> _metadataSession = _metadata.Add(new SessionAttribute { SessionBehavior = SessionStateBehavior.Required });

    public RoutePattern? Pattern { get; init; }

    public string? Path { get; init; }

    public static RouteItem Create<T>(string path)
        where T : IHttpHandler
        => new(new HandlerMetadata(typeof(T)), GetSessionStateBehavior(typeof(T))) { Path = path };

    public static RouteItem Create(IHttpHandler handler)
        => Create(handler, string.Empty);

    public static RouteItem Create(string path, Type type)
    {
        if (!type.IsAssignableTo(typeof(IHttpHandler)))
        {
            throw new InvalidOperationException("Must be of type IHttpHandler");
        }

        return new(new HandlerMetadata(type), GetSessionStateBehavior(type)) { Path = path };
    }

    public static RouteItem Create(IHttpHandler handler, string path)
    {
        var behavior = handler switch
        {
            IReadOnlySessionState => SessionStateBehavior.ReadOnly,
            IRequiresSessionState => SessionStateBehavior.Required,
            _ => SessionStateBehavior.Default,
        };

        return new(new SingletonHandlerMetadata(handler), behavior) { Path = path };
    }

    private RoutePattern? GetPattern()
    {
        if (Pattern is { } pattern)
        {
            return pattern;
        }

        if (Path is { } path)
        {
            return RoutePatternFactory.Parse(path);
        }

        return null;
    }

    public EndpointBuilder GetBuilder()
    {
        EndpointBuilder builder = GetPattern() is { } pattern
            ? new RouteEndpointBuilder(DefaultHandler, pattern, 0)
            : new NonRouteEndpointBuilder() { RequestDelegate = DefaultHandler };

        builder.Metadata.Add(Handler);

        foreach (var m in GetMetadataCollection())
        {
            builder.Metadata.Add(m);
        }

        return builder;
    }

    private static SessionStateBehavior GetSessionStateBehavior(Type type)
    {
        if (type.IsAssignableTo(typeof(IReadOnlySessionState)))
        {
            return SessionStateBehavior.ReadOnly;
        }

        if (type.IsAssignableTo(typeof(IRequiresSessionState)))
        {
            return SessionStateBehavior.Required;
        }

        return SessionStateBehavior.Default;
    }

    private ImmutableList<object> GetMetadataCollection() => SessionState switch
    {
        SessionStateBehavior.ReadOnly => _metadataReadonlySession,
        SessionStateBehavior.Required => _metadataSession,
        _ => _metadata
    };

    private static Task DefaultHandler(HttpContextCore context)
    {
        if (context.Features.GetRequired<IHttpHandlerFeature>().Current is { } handler)
        {
            return handler.RunHandlerAsync(context).AsTask();
        }

        context.Response.StatusCode = 500;
        return context.Response.WriteAsync("Invalid handler");
    }

    sealed class HandlerMetadata(Type type) : IHttpHandlerMetadata
    {
        private IHttpHandler? _handler;
        private readonly ObjectFactory _factory = ActivatorUtilities.CreateFactory(type, []);

        public ValueTask<IHttpHandler> Create(HttpContextCore context)
        {
            if (_handler is { } h)
            {
                return ValueTask.FromResult(h);
            }

            var newHandler = (IHttpHandler)_factory(context.RequestServices, null);

            if (newHandler.IsReusable)
            {
                Interlocked.Exchange(ref _handler, newHandler);
            }

            return ValueTask.FromResult(newHandler);
        }
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

    private sealed class SingletonHandlerMetadata(IHttpHandler handler) : IHttpHandlerMetadata
    {
        public ValueTask<IHttpHandler> Create(HttpContextCore context) => ValueTask.FromResult(handler);
    }
}
