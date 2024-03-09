// MIT License.

#nullable enable

using System.Diagnostics.CodeAnalysis;
using System.Web;
using System.Web.UI;
using System.Web.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace WebForms.Internal;

internal interface ICompiledTypeAccessor
{
    Type? GetForPath(string virtualPath);
}

/// <summary>
/// A string comparer comparer that ignores any '~' or '/' at the beginning.
/// </summary>
internal sealed class PathComparer : IEqualityComparer<string>
{
    private PathComparer()
    {
    }

    public static IEqualityComparer<string> Instance { get; } = new PathComparer();

    public bool Equals(string? x, string? y)
    {
        if (x is null && y is null)
        {
            return true;
        }

        if (x is null || y is null)
        {
            return false;
        }

        return Normalized(x).Equals(Normalized(y), StringComparison.OrdinalIgnoreCase);
    }

    public int GetHashCode([DisallowNull] string obj)
        => string.GetHashCode(Normalized(obj), StringComparison.OrdinalIgnoreCase);

    private static ReadOnlySpan<char> Normalized(string s) => s.AsSpan().TrimStart("~/");
}

internal static class CompiledTypeAccessExtensions
{
    public static Control? GetControlByPath(this System.Web.HttpContext context, string virtualPath) => context.AsAspNetCore().GetControlByPath(virtualPath);

    public static Control? GetControlByPath(this HttpContextCore context, string virtualPath)
    {
        var type = context.GetRequiredCompiledTypes().GetForPath(virtualPath);

        if (type is null)
        {
            context.RequestServices.GetRequiredService<ILogger<ICompiledTypeAccessor>>().LogError("Type for {VirtualPath} could not be found", virtualPath);
            return null;
        }

        if (!type.IsAssignableTo(typeof(Control)))
        {
            context.RequestServices.GetRequiredService<ILogger<ICompiledTypeAccessor>>().LogError("Path {VirtualPath} is not a valid control", virtualPath);
            return null;
        }

        return (Control)ActivatorUtilities.CreateInstance(context.RequestServices, type);
    }

    public static ICompiledTypeAccessor GetCompiledTypes(this System.Web.HttpContext context) => context.AsAspNetCore().GetRequiredCompiledTypes();

    public static ITypedWebObjectFactory? GetTypedWebObjectForPath(this System.Web.HttpContext context, VirtualPath path)
    {
        var ctx = context.AsAspNetCore();
        var type = ctx.GetRequiredCompiledTypes().GetForPath(path.Path);

        if (type is null)
        {
            ctx.RequestServices.GetRequiredService<ILogger<ICompiledTypeAccessor>>().LogError("Type for {VirtualPath} could not be found", path.Path);
            return null;
        }

        return new ActivatedType(type, ctx.RequestServices);
    }

    private sealed class ActivatedType(Type type, IServiceProvider services) : ITypedWebObjectFactory
    {
        public Type InstantiatedType => type;

        public object CreateInstance() => ActivatorUtilities.CreateInstance(services, type);
    }

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
