// MIT License.

using System.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.SystemWebAdapters.HttpHandlers;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Microsoft.Extensions.DependencyInjection;

public static class HandlerServicesExtensions
{
    public static ISystemWebAdapterBuilder AddHttpHandler<T>(this ISystemWebAdapterBuilder services, string route)
        where T : IHttpHandler
    {
        services.AddHttpHandlers();
        services.Services.TryAddSingleton<IHttpHandlerCollection, RegisteredHandlerCollection>();
        services.Services.AddOptions<RegisteredHandlerOptions>()
            .Configure(options => options.Metadata.Add(HandlerMetadata.Create<T>(route)));

        return services;
    }

    public static ISystemWebAdapterBuilder AddHttpHandlers(this ISystemWebAdapterBuilder services)
    {
        services.Services.TryAddSingleton<IHttpHandlerEndpointFactory, HttpHandlerEndpointFactory>();
        services.Services.TryAddEnumerable(ServiceDescriptor.Transient<IStartupFilter, HttpHandlerStartupFilter>());
        services.Services.TryAddTransient<HandlerMetadataProvider>();
        services.Services.TryAddSingleton<HttpHandlerEndpointConventionBuilder>();

        return services;
    }

    private sealed class HttpHandlerStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
            => builder =>
            {
                builder.UseMiddleware<SetHttpHandlerMiddleware>();
                next(builder);
            };
    }

    private sealed class RegisteredHandlerCollection(IOptions<RegisteredHandlerOptions> options) : IHttpHandlerCollection
    {
        IEnumerable<NamedHttpHandlerRoute> IHttpHandlerCollection.NamedRoutes => [];

        IChangeToken IHttpHandlerCollection.GetChangeToken() => NullChangeToken.Singleton;

        IEnumerable<IHttpHandlerMetadata> IHttpHandlerCollection.GetHandlerMetadata() => options.Value.Metadata;
    }

    private sealed class RegisteredHandlerOptions
    {
        public List<IHttpHandlerMetadata> Metadata { get; } = [];
    }
}
