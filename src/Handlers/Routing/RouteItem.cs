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

internal sealed record RouteItem(Func<HttpContextCore, IHttpHandler> Handler, Type Type)
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
        => new(CreateActivator(typeof(T)), typeof(T)) { Path = path };

    public static RouteItem Create(string path, Type type)
    {
        if (!type.IsAssignableTo(typeof(IHttpHandler)))
        {
            throw new InvalidOperationException("Must be of type IHttpHandler");
        }

        return new(CreateActivator(type), type) { Path = path };
    }

    public static RouteItem Create(IHttpHandler handler, string path)
       => new(_ => handler, handler.GetType()) { Path = path };

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

    private ImmutableList<object> GetMetadataCollection()
    {
        if (Type.IsAssignableTo(typeof(IReadOnlySessionState)))
        {
            return _metadataReadonlySession;
        }

        if (Type.IsAssignableTo(typeof(IRequiresSessionState)))
        {
            return _metadataSession;
        }

        return _metadata;
    }

    private static Task DefaultHandler(HttpContextCore context)
    {
        if (context.Features.GetRequired<IHttpHandlerFeature>().Current is { } handler)
        {
            return handler.RunHandlerAsync(context);
        }

        context.Response.StatusCode = 500;
        return context.Response.WriteAsync("Invalid handler");
    }

    private static Func<HttpContextCore, IHttpHandler> CreateActivator(Type type)
    {
        var factory = ActivatorUtilities.CreateFactory(type, []);

        return CreateActivator(factory);

        static Func<HttpContextCore, IHttpHandler> CreateActivator(ObjectFactory factory)
        {
            IHttpHandler? handler = null;

            return (HttpContextCore context) =>
            {
                if (handler is { } h)
                {
                    return h;
                }

                var newHandler = (IHttpHandler)factory(context.RequestServices, null);

                if (newHandler.IsReusable)
                {
                    Interlocked.Exchange(ref handler, newHandler);
                }

                return newHandler;
            };
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
}
