// MIT License.

#nullable enable

using Microsoft.AspNetCore.SystemWebAdapters;

namespace System.Web;

internal static class HttpContextExtensions
{
    public static T? GetFeature<T>(this HttpContext context)
        => context.AsAspNetCore().Features.Get<T>();

    public static T GetRequiredFeature<T>(this HttpContext context)
        => context.AsAspNetCore().Features.Get<T>() ?? throw new InvalidOperationException($"Feature '{typeof(T).FullName}' is not available");
}
