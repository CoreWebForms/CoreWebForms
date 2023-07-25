// MIT License.

using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using System.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

public static class HandlerServicesExtensions
{
    public static ISystemWebAdapterBuilder AddHttpHandlers(this ISystemWebAdapterBuilder services, Action<HttpHandlerOptions>? configure = null)
    {
        services.Services.TryAddSingleton<IHttpHandlerEndpointFactory, HttpHandlerEndpointFactory>();
        services.Services.AddTransient<IStartupFilter, HttpHandlerStartupFilter>();
        services.Services.AddTransient(_ => new System.Web.Routing.RouteCollection());

        if (configure is not null)
        {
            services.Services.AddOptions<HttpHandlerOptions>()
                .Configure(configure);
        }

        return services;
    }

    public static IEndpointConventionBuilder MapWebFormsPages(this IEndpointRouteBuilder endpoints)
    {
        var env = endpoints.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
        var httpHandlers = endpoints.MapHttpHandlers();

        if (GetWebFormsFile(env) is { Exists: true } file)
        {
            var routes = endpoints.ServiceProvider.GetRequiredService<IOptions<HttpHandlerOptions>>().Value.Routes;
            var results = JsonSerializer.Deserialize<WebFormsDetails[]>(file.CreateReadStream());
            var context = GetLoadContext();

            if (results is not null)
            {
                foreach (var type in results)
                {
                    if (context.LoadFromAssemblyName(new AssemblyName($"{type.Assembly}")).GetType(type.Type) is { } pageType)
                    {
                        routes.Add(type.Path, pageType);
                    }
                }
            }
        }

        return httpHandlers;
    }

    private static AssemblyLoadContext GetLoadContext()
        => AssemblyLoadContext.All.OfType<WebFormsAssemblyLoadContext>().FirstOrDefault() ?? new WebFormsAssemblyLoadContext();

    private sealed class WebFormsAssemblyLoadContext : AssemblyLoadContext
    {
        public WebFormsAssemblyLoadContext()
            : base("WebForms Load Context")
        {
        }

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            if (assemblyName.Name is { } name && name.StartsWith("WebForms.ASP.", StringComparison.OrdinalIgnoreCase))
            {
                var path = Path.Combine(AppContext.BaseDirectory, $"{name}.dll");

                if (File.Exists(path))
                {
                    return LoadFromAssemblyPath(path);
                }
            }

            return null;
        }
    }

    private static IFileInfo? GetWebFormsFile(IWebHostEnvironment env)
    {
        const string DetailsPath = "webforms.pages.json";

        if (env.ContentRootFileProvider.GetFileInfo(DetailsPath) is { Exists: true } file)
        {
            return file;
        }

        if (env.IsDevelopment() && new PhysicalFileProvider(AppContext.BaseDirectory).GetFileInfo(DetailsPath) is { } debug)
        {
            return debug;
        }

        return null;
    }

    private sealed record WebFormsDetails(string Path, string Type, string Assembly);

    public static IEndpointConventionBuilder MapHttpHandlers(this IEndpointRouteBuilder endpoints)
    {
        if (endpoints.DataSources.OfType<HttpHandlerEndpointConventionBuilder>().FirstOrDefault() is { } existing)
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
                next(builder);
            };
    }
}
