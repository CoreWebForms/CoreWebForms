// MIT License.

#nullable enable

namespace System.Web;

internal static class HttpContextExtensions
{
    public static Microsoft.AspNetCore.Http.HttpContext AsCore(this HttpContext context) => context;

    public static T? GetFeature<T>(this HttpContext context)
        => context.AsCore().Features.Get<T>();

    public static T GetRequiredFeature<T>(this HttpContext context)
        => context.AsCore().Features.Get<T>() ?? throw new InvalidOperationException($"Feature '{typeof(T).FullName}' is not available");
}
