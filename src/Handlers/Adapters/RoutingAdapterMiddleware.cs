// MIT License.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal sealed class RoutingAdapterMiddleware
{
    private readonly RequestDelegate _next;

    public RoutingAdapterMiddleware(RequestDelegate next) => _next = next;

    public Task InvokeAsync(HttpContextCore context)
    {
        if (context.Features.Get<IRoutingFeature>() is { } feature)
        {
            var newFeature = new RoutingAdapterFeature(feature);

            context.Features.Set<IRoutingFeature>(newFeature);
            context.Features.Set<IRoutingAdapterFeature>(newFeature);
        }

        return _next(context);
    }
}
