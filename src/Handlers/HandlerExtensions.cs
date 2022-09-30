// MIT License.

using System.Collections.Immutable;
using System.Web;
using System.Web.SessionState;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.SystemWebAdapters;

public static class HandlerExtensions
{
    private static readonly ImmutableList<object> _metadata = new object[]
    {
        new BufferResponseStreamAttribute(),
        new PreBufferRequestStreamAttribute(),
        new SetThreadCurrentPrincipalAttribute(),
        new SingleThreadedRequestAttribute(),
    }.ToImmutableList();

    private static readonly ImmutableList<object> _metadataReadonlySession = _metadata.Add(new SessionAttribute { IsReadOnly = true });
    private static readonly ImmutableList<object> _metadataSession = _metadata.Add(new SessionAttribute { IsReadOnly = false });

    public static EndpointBuilder AddHttpHandler<THandler>(this EndpointBuilder builder)
        where THandler : IHttpHandler
        => builder.AddHttpHandler(typeof(THandler));

    public static EndpointBuilder AddHttpHandler(this EndpointBuilder builder, IHttpHandler handler)
        => builder.AddHttpHandler(_ => handler, handler.GetType());

    public static EndpointBuilder AddHttpHandler(this EndpointBuilder builder, Type type)
    {
        if (!type.IsAssignableTo(typeof(IHttpHandler)))
        {
            throw new InvalidOperationException($"Type {type} is not a valid IHttpHandler type");
        }

        var factory = ActivatorUtilities.CreateFactory(type, Array.Empty<Type>());

        return builder.AddHttpHandler(CreateActivator(factory), type);

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

    private static EndpointBuilder AddHttpHandler(this EndpointBuilder builder, Func<HttpContextCore, IHttpHandler> factory, Type type)
    {
        builder.Metadata.Add(factory);

        builder.RequestDelegate = (HttpContextCore context) =>
        {
            if (context.Features.GetRequired<IHttpHandlerFeature>().Current is { } handler)
            {
                return handler.RunHandlerAsync(context);
            }

            context.Response.StatusCode = 500;
            return context.Response.WriteAsync("Invalid handler");
        };

        foreach (var item in GetMetadataCollection(type))
        {
            builder.Metadata.Add(item);
        }

        builder.DisplayName = type.FullName;

        return builder;

        static ImmutableList<object> GetMetadataCollection(Type type)
        {
            if (type.IsAssignableTo(typeof(IReadOnlySessionState)))
            {
                return _metadataReadonlySession;
            }

            if (type.IsAssignableTo(typeof(IRequiresSessionState)))
            {
                return _metadataSession;
            }

            return _metadata;
        }
    }

    private static async Task RunHandlerAsync(this IHttpHandler handler, HttpContextCore context)
    {
        if (handler is HttpTaskAsyncHandler task)
        {
            await task.ProcessRequestAsync(context).ConfigureAwait(false);
        }
        else if (handler is IHttpAsyncHandler asyncHandler)
        {
            await Task.Factory.FromAsync((cb, state) => asyncHandler.BeginProcessRequest(context, cb, state), asyncHandler.EndProcessRequest, null).ConfigureAwait(false);
        }
        else
        {
            handler.ProcessRequest(context);
        }
    }

    internal static Endpoint CreateEndpoint(this HttpContextCore core, IHttpHandler handler)
    {
        if (handler is Endpoint endpoint)
        {
            return endpoint;
        }

        var factory = core.RequestServices.GetRequiredService<IHttpHandlerEndpointFactory>();

        return factory.Create(handler);
    }

    internal static IHttpHandler CreateHandler(this HttpContextCore context, Endpoint endpoint)
    {
        if (endpoint is IHttpHandler handler)
        {
            return handler;
        }
        else if (endpoint.Metadata.GetMetadata<Func<HttpContextCore, IHttpHandler>>() is { } factory)
        {
            return factory(context);
        }
        else
        {
            return new EndpointHandler(endpoint);
        }
    }

    private sealed class EndpointHandler : HttpTaskAsyncHandler
    {
        public EndpointHandler(Endpoint endpoint)
        {
            Endpoint = endpoint;
        }

        public Endpoint Endpoint { get; }

        public override Task ProcessRequestAsync(System.Web.HttpContext context)
        {
            if (Endpoint.RequestDelegate is { } request)
            {
                return request(context);
            }

            return Task.CompletedTask;
        }
    }
}

