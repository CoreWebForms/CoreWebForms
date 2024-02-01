// MIT License.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SystemWebAdapters.Features;

namespace Microsoft.AspNetCore.SystemWebAdapters.HttpHandlers;

internal sealed class SetHttpHandlerMiddleware
{
    private readonly RequestDelegate _next;

    public SetHttpHandlerMiddleware(RequestDelegate next) => _next = next;

    public Task InvokeAsync(HttpContext context)
    {
        var feature = new HttpHandlerEndpointFeature(context);

        context.Features.Set<IHttpHandlerFeature>(feature);
        context.Features.Set<IEndpointFeature>(feature);

        return _next(context);
    }
}
