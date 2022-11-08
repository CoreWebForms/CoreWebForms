// MIT License.

using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal static class HandlerExtensions
{
    internal static async Task RunHandlerAsync(this IHttpHandler handler, HttpContextCore context)
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

