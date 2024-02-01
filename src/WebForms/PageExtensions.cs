// MIT License.

using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using System.Web;
using System.Web.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.Builder;

public static class PageExtensions
{
    public static ISystemWebAdapterBuilder AddWebForms(this ISystemWebAdapterBuilder builder)
    {
        builder.AddHttpHandlers();
        builder.AddRouting();

        return builder;
    }

    public static IEndpointConventionBuilder MapWebForms(this IEndpointRouteBuilder endpoints)
    {
        if (endpoints.DataSources.OfType<WebFormsConventionBuilder>().FirstOrDefault() is { } existing)
        {
            return existing;
        }

        const string Prefix = "/__webforms/scripts/system.web";

        var provider = new EmbeddedFileProvider(typeof(Page).Assembly, "System.Web.UI.WebControls.RuntimeScripts");
        var files = new StaticFileOptions
        {
            RequestPath = Prefix,
            FileProvider = provider,
            OnPrepareResponse = SetCacheHeaders,
        };

        var app = endpoints.CreateApplicationBuilder();

        app.Use((context, next) =>
        {
            context.SetEndpoint(null);
            return next(context);
        });

        app.UseStaticFiles(files);

        var pipeline = app.Build();

        endpoints.MapHttpHandlers();
        endpoints.MapWebFormsPages();

        var composite = new WebFormsConventionBuilder();

        foreach (var file in provider.GetDirectoryContents(string.Empty))
        {
            if (file is IFileInfo f)
            {
                composite.Add(endpoints.MapGet($"{Prefix}/{f.Name}", pipeline)
                    .WithDisplayName($"WebForms Static Files [{f.Name}]"));
            }
        }

        ((IEndpointConventionBuilder)composite).Add(builder => ((RouteEndpointBuilder)builder).Order = int.MinValue);

        return composite;
    }

    private static void SetCacheHeaders(StaticFileResponseContext context)
    {
        // By setting "Cache-Control: no-cache", we're allowing the browser to store
        // a cached copy of the response, but telling it that it must check with the
        // server for modifications (based on Etag) before using that cached copy.
        // Longer term, we should generate URLs based on content hashes (at least
        // for published apps) so that the browser doesn't need to make any requests
        // for unchanged files.
        var headers = context.Context.Response.GetTypedHeaders();
        if (headers.CacheControl == null)
        {
            headers.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true
            };
        }
    }

    // This is an empty endpoint so that we can track it in the IEndpointRouteBuilder
    private sealed class WebFormsConventionBuilder : EndpointDataSource, IWebFormsEndpointConventionBuilder
    {
        private readonly List<IEndpointConventionBuilder> _builders = new();

        public override IReadOnlyList<Endpoint> Endpoints => [];

        public void Add(IEndpointConventionBuilder builder)
        {
            _builders.Add(builder);
        }

        public override IChangeToken GetChangeToken() => NullChangeToken.Singleton;

        void IEndpointConventionBuilder.Add(Action<EndpointBuilder> convention)
        {
            foreach (var builder in _builders)
            {
                builder.Add(convention);
            }
        }
    }

    private static IEndpointConventionBuilder MapWebFormsPages(this IEndpointRouteBuilder endpoints)
    {
        var env = endpoints.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

        IEndpointConventionBuilder builder = default;

        if (GetWebFormsFile(env) is { Exists: true } file)
        {
            var results = JsonSerializer.Deserialize<WebFormsDetails[]>(file.CreateReadStream());
            var context = GetLoadContext();

            if (results is not null)
            {
                foreach (var type in results)
                {
                    if (context.LoadFromAssemblyName(new AssemblyName($"{type.Assembly}")).GetType(type.Type) is { } pageType)
                    {
                        builder = endpoints.MapHttpHandler(type.Path, pageType);
                    }
                }
            }
        }

        return builder ?? new EmptyConventionBuilder();
    }

    private class EmptyConventionBuilder : IEndpointConventionBuilder
    {
        public void Add(Action<EndpointBuilder> convention)
        {
        }

        return builder ?? new EmptyConventionBuilder();
    }

    private class EmptyConventionBuilder : IEndpointConventionBuilder
    {
        public void Add(Action<EndpointBuilder> convention)
        {
        }
    }

    private static AssemblyLoadContext GetLoadContext()
        => AssemblyLoadContext.All.OfType<WebFormsAssemblyLoadContext>().FirstOrDefault() ?? new WebFormsAssemblyLoadContext();

    private sealed class WebFormsAssemblyLoadContext : AssemblyLoadContext
    {
        public WebFormsAssemblyLoadContext()
            : base("WebForms Load Context")
        {
        }

        protected override Assembly Load(AssemblyName assemblyName)
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

    private static IFileInfo GetWebFormsFile(IWebHostEnvironment env)
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

}
