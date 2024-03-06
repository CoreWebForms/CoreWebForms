// MIT License.

#nullable enable

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace WebForms.Internal;

internal interface ICompiledTypeAccessor
{
    Type? GetForPath(string virtualPath);

    Type GetRequiredType(string virtualPath) => GetForPath(virtualPath) ?? throw new InvalidOperationException($"Type not available for {virtualPath}");
}

internal static class CompiledTypeAccessExtensions
{
    public static ICompiledTypeAccessor GetCompiledTypes(this System.Web.HttpContext context) => context.AsAspNetCore().GetRequiredCompiledTypes();

    public static ICompiledTypeAccessor GetRequiredCompiledTypes(this HttpContextCore context) => context.GetCompiledTypes() ?? throw new InvalidOperationException("Compiled types not available");

    public static ICompiledTypeAccessor? GetCompiledTypes(this HttpContextCore context)
    {
        if (context.Features.Get<ICompiledTypeAccessor>() is { } feature)
        {
            return feature;
        }
        var metadata = context.GetEndpoint()?.Metadata;
        var accessor = metadata?.GetMetadata<ICompiledTypeAccessor>();

        if (accessor is null)
        {
            return null;
        }

        feature = new ContextWrappedAccessor(context, accessor);
        context.Features.Set<ICompiledTypeAccessor>(feature);
        return feature;
    }

    private sealed class ContextWrappedAccessor(HttpContextCore context, ICompiledTypeAccessor other) : ICompiledTypeAccessor
    {
        private readonly ILogger _logger = context.RequestServices.GetRequiredService<ILogger<ContextWrappedAccessor>>();

        Type? ICompiledTypeAccessor.GetForPath(string virtualPath)
        {
            _logger.LogDebug($"{context.Request.Path} is searching for compiled type {virtualPath}");

            return other.GetForPath(virtualPath);
        }
    }
}
