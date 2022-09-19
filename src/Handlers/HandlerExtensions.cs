// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Web.SessionState;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using HttpContextCore = Microsoft.AspNetCore.Http.HttpContext;

namespace System.Web;

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

    public static void AddHttpHandlers(this ISystemWebAdapterBuilder services)
    {
        services.Services.TryAddSingleton<EndpointCache>();
    }

    public static void UseHttpHandlers(this IApplicationBuilder app)
    {
        app.UseMiddleware<SetHttpHandlerMiddleware>();
    }

    public static void SetHandler(this HttpContext context, IHttpHandler handler)
    {
        var coreContext = (Microsoft.AspNetCore.Http.HttpContext)context;
        coreContext.Features.GetRequired<IHttpHandlerFeature>().Current = handler;
    }

    internal static T GetRequired<T>(this IFeatureCollection features)
        => features.Get<T>() ?? throw new InvalidOperationException();

    public static EndpointBuilder AddHttpHandler<THandler>(this EndpointBuilder builder)
        where THandler : IHttpHandler
        => builder.AddHttpHandler(typeof(THandler));

    public static EndpointBuilder AddHttpHandler(this EndpointBuilder builder, IHttpHandler handler)
    {
        builder.Metadata.Add((HttpContextCore context) => handler);
        return builder;
    }

    public static EndpointBuilder AddHttpHandler(this EndpointBuilder builder, Type type)
    {
        if (!type.IsAssignableTo(typeof(IHttpHandler)))
        {
            throw new InvalidOperationException($"Type {type} is not a valid IHttpHandler type");
        }

        var factory = ActivatorUtilities.CreateFactory(type, Array.Empty<Type>());
        builder.Metadata.Add(CreateActivator(factory));

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

    internal class EndpointCache
    {
        private readonly ConditionalWeakTable<IHttpHandler, Endpoint> _table = new();

        public bool TryGetValue(IHttpHandler handler, [NotNullWhen(true)] out Endpoint? existing)
            => _table.TryGetValue(handler, out existing);

        public void Add(IHttpHandler handler, Endpoint newEndpoint)
            => _table.Add(handler, newEndpoint);
    }

    internal static Endpoint CreateEndpoint(this HttpContextCore core, IHttpHandler handler)
    {
        if (handler is Endpoint endpoint)
        {
            return endpoint;
        }

        var cache = core.RequestServices.GetRequiredService<EndpointCache>();

        if (cache.TryGetValue(handler, out var existing))
        {
            return existing;
        }

        var newEndpoint = new HandlerEndpointBuilder()
            .AddHttpHandler(handler)
            .Build();

        cache.Add(handler, newEndpoint);

        return newEndpoint;
    }

    private class HandlerEndpointBuilder : EndpointBuilder
    {
        public override Endpoint Build() => new(RequestDelegate, new(Metadata), DisplayName);
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
