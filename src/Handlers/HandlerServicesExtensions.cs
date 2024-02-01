// MIT License.

using System.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.SystemWebAdapters.HttpHandlers;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class HandlerServicesExtensions
{
    public static ISystemWebAdapterBuilder AddHttpHandlers(this ISystemWebAdapterBuilder services)
    {
        services.Services.TryAddSingleton<IHttpHandlerEndpointFactory, HttpHandlerEndpointFactory>();
        services.Services.AddTransient<IStartupFilter, HttpHandlerStartupFilter>();

        return services;
    }

    public static IEndpointConventionBuilder MapHttpHandlers(this IEndpointRouteBuilder endpoints)
    {
        if (endpoints.DataSources.OfType<ServiceHttpHandlerEndpointConventionBuilder>().FirstOrDefault() is { } existing)
        {
            return existing;
        }

        var source = new ServiceHttpHandlerEndpointConventionBuilder(endpoints.ServiceProvider);

        endpoints.DataSources.Add(source);

        return source;
    }

    private sealed class ServiceHttpHandlerEndpointConventionBuilder(IServiceProvider services)
        : HttpHandlerEndpointConventionBuilder(services.GetRequiredService<IHttpHandlerManager>());

    private sealed class HttpHandlerStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
            => builder =>
            {
                builder.UseMiddleware<SetHttpHandlerMiddleware>();
                next(builder);
            };
    }
}
