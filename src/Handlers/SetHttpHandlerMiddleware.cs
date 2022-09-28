// MIT License.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal sealed class SetHttpHandlerMiddleware
{
    private readonly RequestDelegate _next;

    public SetHttpHandlerMiddleware(RequestDelegate next) => _next = next;

    public Task InvokeAsync(HttpContext context)
    {
        var feature = new HttpHandlerEndpointFeature(context, context.Features.Get<IEndpointFeature>());

        context.Features.Set<IHttpHandlerFeature>(feature);
        context.Features.Set<IEndpointFeature>(feature);

        return _next(context);
    }
}
