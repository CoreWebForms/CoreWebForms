// MIT License.

using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SystemWebAdapters.Features;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.SystemWebAdapters.HttpHandlers;

internal sealed class HttpHandlerEndpointFeature : IHttpHandlerFeature, IEndpointFeature
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

    private struct Container(HttpContextCore context, Endpoint? endpoint = null, IHttpHandler? handler = null)
    {
        public Endpoint? Endpoint
        {
            get
            {
                if (endpoint is null)
                {
                    if (handler is null)
                    {
                        return null;
                    }

                    endpoint = CreateEndpoint(context, handler);
                }

                return endpoint;
            }
        }

        public IHttpHandler? Handler
        {
            get
            {
                if (handler is null)
                {
                    if (endpoint is null)
                    {
                        return null;
                    }

                    handler = CreateHandler(context, endpoint);
                }

                return handler;
            }
        }

        private static Endpoint CreateEndpoint(HttpContextCore core, IHttpHandler handler)
        {
            if (handler is Endpoint endpoint)
            {
                return endpoint;
            }

            var factory = core.RequestServices.GetRequiredService<IHttpHandlerEndpointFactory>();

            return factory.Create(handler);
        }

        private static IHttpHandler CreateHandler(HttpContextCore context, Endpoint endpoint)
        {
            if (endpoint is IHttpHandler handler)
            {
                return handler;
            }
            else if (endpoint.Metadata.GetMetadata<IHttpHandlerMetadata>() is { } metadata)
            {
                return metadata.Create(context);
            }
            else
            {
                return new EndpointHandler(endpoint);
            }
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

