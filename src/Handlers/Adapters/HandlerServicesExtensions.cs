// MIT License.

using System.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

public static class HandlerServicesExtensions
{
    public static ISystemWebAdapterBuilder AddHttpHandlers(this ISystemWebAdapterBuilder services, Action<HttpHandlerOptions>? configure = null)
    {
        services.Services.TryAddSingleton<IHttpHandlerEndpointFactory, HttpHandlerEndpointFactory>();
        services.Services.AddTransient<IStartupFilter, HttpHandlerStartupFilter>();
        services.Services.AddSingleton(_ => new HttpApplicationState());
        services.Services.AddTransient(_ => new System.Web.Routing.RouteCollection());

        if (configure is not null)
        {
            services.Services.AddOptions<HttpHandlerOptions>()
                .Configure(configure);
        }

        return services;
    }

    public static IHttpHandlerEndpointConventionBuilder MapHttpHandlers(this IEndpointRouteBuilder endpoints)
    {
        if (endpoints.DataSources.OfType<IHttpHandlerEndpointConventionBuilder>().FirstOrDefault() is { } existing)
        {
            return existing;
        }

        var source = new HttpHandlerEndpointConventionBuilder(endpoints.ServiceProvider.GetRequiredService<IOptions<HttpHandlerOptions>>().Value.Routes);

        endpoints.DataSources.Add(source);

        return source;
    }

    private sealed class HttpHandlerStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
            => builder =>
            {
                builder.UseMiddleware<SetHttpHandlerMiddleware>();
                builder.UseMiddleware<RoutingAdapterMiddleware>();
                next(builder);
            };
    }

    public static void UseSystemWebAdapters2(this IApplicationBuilder builder)
    {
        //builder.UseMiddleware<RoutingAdapterMiddleware>();
    }
}
