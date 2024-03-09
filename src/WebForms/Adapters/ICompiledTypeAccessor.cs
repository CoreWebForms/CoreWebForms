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

public interface ICompiledTypeAccessor
{
    IReadOnlyCollection<string> Paths { get; }

    bool TryGetException(string path, [MaybeNullWhen(false)] out Exception exception);

    Type? GetForPath(string virtualPath);
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
        feature = metadata?.GetMetadata<ICompiledTypeAccessor>();

        if (feature is null)
        {
            return null;
        }

        context.Features.Set(feature);
        return feature;
    }
}
