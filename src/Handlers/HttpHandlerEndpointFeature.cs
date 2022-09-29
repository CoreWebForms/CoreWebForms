// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal class HttpHandlerEndpointFeature : IHttpHandlerFeature, IEndpointFeature
{
    private readonly HttpContextCore _context;

    private Container _current;
    private Container _previous;

    public HttpHandlerEndpointFeature(HttpContextCore context)
    {
        _context = context;
        _current = new(_context, endpoint: context.Features.Get<IEndpointFeature>()?.Endpoint);
    }

    Endpoint? IEndpointFeature.Endpoint
    {
        get => _current.Endpoint;
        set
        {
            _previous = _current;
            _current = new(_context, endpoint: value);
        }
    }

    IHttpHandler? IHttpHandlerFeature.Current
    {
        get => _current.Handler;
        set
        {
            _previous = _current;
            _current = new(_context, handler: value);
        }
    }

    IHttpHandler? IHttpHandlerFeature.Previous => _previous.Handler;

    private struct Container
    {
        private readonly HttpContextCore _context;

        private Endpoint? _endpoint;
        private IHttpHandler? _handler;

        public Container(HttpContextCore context, Endpoint? endpoint = null, IHttpHandler? handler = null)
        {
            _context = context;
            _endpoint = endpoint;
            _handler = handler;
        }

        public Endpoint? Endpoint
        {
            get
            {
                if (_endpoint is null)
                {
                    if (_handler is null)
                    {
                        return null;
                    }

                    _endpoint = _context.CreateEndpoint(_handler);
                }

                return _endpoint;
            }
        }

        public IHttpHandler? Handler
        {
            get
            {
                if (_handler is null)
                {
                    if (_endpoint is null)
                    {
                        return null;
                    }

                    _handler = _context.CreateHandler(_endpoint);
                }

                return _handler;
            }
        }
    }
}

