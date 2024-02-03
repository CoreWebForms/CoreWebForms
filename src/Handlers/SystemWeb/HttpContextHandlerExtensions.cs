// MIT License.

using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.SystemWebAdapters.Features;

namespace System.Web;

public static class HttpContextHandlerExtensions
{
    public static void SetHandler(this HttpContext context, IHttpHandler handler)
        => ((HttpContextCore)context).Features.GetRequiredFeature<IHttpHandlerFeature>().Current = handler;

    public static IHttpHandler? GetHandler(this HttpContext context)
        => ((HttpContextCore)context).Features.GetRequiredFeature<IHttpHandlerFeature>().Current;
}
