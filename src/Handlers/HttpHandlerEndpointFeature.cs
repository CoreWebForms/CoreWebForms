// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if NETCOREAPP

using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Microsoft.AspNetCore.SystemWebAdapters;

public class HttpHandlerEndpointFeature : IHttpHandlerFeature, IEndpointFeature
{
    private static readonly ConditionalWeakTable<IHttpHandler, HandlerEndpoint> _table = new();

    private object? _current;
    private object? _previous;

    Endpoint? IEndpointFeature.Endpoint
    {
        get => GetEndpoint(ref _current);
        set => Set(value);
    }

    IHttpHandler? IHttpHandlerFeature.Current
    {
        get => GetHandler(ref _current);
        set => Set(value);
    }

    IHttpHandler? IHttpHandlerFeature.Previous => GetHandler(ref _previous);

    private void Set(object? value)
    {
        _previous = _current;
        _current = value;
    }

    private static Endpoint? GetEndpoint(ref object? obj) => obj switch
    {
        null => null,
        Endpoint endpoint => endpoint,
        IHttpHandler handler => CreateEndpoint(handler, ref obj),
        _ => throw new InvalidOperationException("Handler must be of known type"),
    };

    private static IHttpHandler? GetHandler(ref object? obj) => obj switch
    {
        null => null,
        IHttpHandler handler => handler,
        HandlerEndpoint handlerEndpoint => handlerEndpoint.Handler,
        Endpoint endpoint => CreateHandler(endpoint, ref obj),
        _ => throw new InvalidOperationException("Handler must be of known type"),
    };

    private static IHttpHandler CreateHandler(Endpoint endpoint, ref object? obj)
    {
        var handler = new EndpointHandler(endpoint);
        obj = handler;
        return handler;
    }

    private static Endpoint CreateEndpoint(IHttpHandler handler, ref object? obj)
    {
        var endpoint = Create(handler);
        obj = endpoint;
        return endpoint;
    }

    private static HandlerEndpoint Create(IHttpHandler handler)
    {
        if (_table.TryGetValue(handler, out var existing))
        {
            return existing;
        }

        var newEndpoint = new HandlerEndpoint(handler);

        _table.Add(handler, newEndpoint);

        return newEndpoint;
    }

    private class EndpointHandler : HttpTaskAsyncHandler
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

    public class HandlerEndpoint : Endpoint
    {
        private static readonly ImmutableList<object> _metadata = new object[]
        {
            new BufferResponseStreamAttribute(),
            new PreBufferRequestStreamAttribute(),
            new SetThreadCurrentPrincipalAttribute(),
            new SingleThreadedRequestAttribute(),
        }.ToImmutableList();

        private static readonly EndpointMetadataCollection _collection = new(_metadata);
        private static readonly EndpointMetadataCollection _collectionReadonlySession = new(_metadata.Add(new SessionAttribute { IsReadOnly = true }));
        private static readonly EndpointMetadataCollection _collectionSession = new(_metadata.Add(new SessionAttribute { IsReadOnly = false }));

        public HandlerEndpoint(IHttpHandler handler)
            : base(CreateRequestDelegate(handler), GetMetadataCollection(handler), handler.GetType().FullName)
        {
            Handler = handler;
        }

        public IHttpHandler Handler { get; }

        private static EndpointMetadataCollection GetMetadataCollection(IHttpHandler handler) => handler switch
        {
            IReadOnlySessionState => _collectionReadonlySession,
            IRequiresSessionState => _collectionSession,
            _ => _collection,
        };

        private static RequestDelegate CreateRequestDelegate(IHttpHandler handler)
            => context => ProcessHandlerAsync(handler, context);

        private static async Task ProcessHandlerAsync(IHttpHandler handler, System.Web.HttpContext context)
        {
            if (handler is HttpTaskAsyncHandler task)
            {
                await task.ProcessRequestAsync(context);
            }
            else if (handler is IHttpAsyncHandler asyncHandler)
            {
                await Task.Factory.FromAsync((cb, state) => asyncHandler.BeginProcessRequest(context, cb, state), asyncHandler.EndProcessRequest, null);
            }
            else
            {
                handler.ProcessRequest(context);
            }
        }
    }
}

#endif
