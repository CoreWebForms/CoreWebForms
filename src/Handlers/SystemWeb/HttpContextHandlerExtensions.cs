// MIT License.

using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.SystemWebAdapters.Features;

namespace System.Web;

public static class HttpContextHandlerExtensions
{
    public static void SetHandler(this HttpContext context, IHttpHandler handler)
        => context.AsAspNetCore().Features.GetRequiredFeature<IHttpHandlerFeature>().Current = handler;

    public static IHttpHandler? GetHandler(this HttpContext context)
        => context.AsAspNetCore().Features.GetRequiredFeature<IHttpHandlerFeature>().Current;
}
