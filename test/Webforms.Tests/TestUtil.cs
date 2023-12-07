// MIT License.

using System.Diagnostics;
using System.Web.UI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Moq;

namespace WebForms.Tests;
internal class TestUtil
{
    internal static async Task<string> RunPage<TPage>(Action<HttpHandlerOptions> configureRoute = null,
    HttpContext defaultContext = null)
        where TPage : Page
    {
        var services = new ServiceCollection();
        var server = new Mock<IServer>();
        server.Setup(s => s.Features).Returns(new FeatureCollection());
        services.AddSingleton(new Mock<IConfiguration>().Object);
        services.AddSingleton(server.Object);
       services.AddSingleton<IHostEnvironment>(new Env());
        services.AddSingleton(new DiagnosticListener(Guid.NewGuid().ToString()));
        services.AddLogging();
        services.AddOptions();
        services.AddRouting();
        services.AddSystemWebAdapters()
            .AddHttpHandlers(options =>
            {
                options.Routes.Add<TPage>("/path"); // Always default.
                if (configureRoute is not null)
                {
                    configureRoute(options);
                }
            });

        var provider = services.BuildServiceProvider();
        using (provider)
        {
            var pipeline = CreatePipeline(provider, app =>
            {
                app.UseRouting();

                app.UseSystemWebAdapters();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapHttpHandlers();
                });

            });
            var body = new MemoryStream();

            if (defaultContext == null)
            {
                var httpContext = new DefaultHttpContext();
                httpContext.Request.Path = "/path";
                httpContext.Response.Body = body;
                httpContext.RequestServices = provider;
                defaultContext = httpContext;
            }
            else
            {
                defaultContext.RequestServices = provider;
                defaultContext.Response.Body = body;
            }
            await pipeline(defaultContext).ConfigureAwait(false);

            body.Position = 0;
            using var reader = new StreamReader(body);
            return reader.ReadToEnd();
        }

    }

    private static RequestDelegate CreatePipeline(IServiceProvider provider, Action<IApplicationBuilder> configure)
    {
        var startupFilters = provider.GetService<IEnumerable<IStartupFilter>>();

        if (startupFilters is not null)
        {
            foreach (var filter in startupFilters.Reverse())
            {
                configure = filter.Configure(configure);
            }
        }

        var builder = new ApplicationBuilder(provider);
        configure(builder);
        return builder.Build();
    }

    private sealed class Env : IHostEnvironment
    {
        public Env()
        {
            ContentRootPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(ContentRootPath);
            ContentRootFileProvider = new PhysicalFileProvider(ContentRootPath);
        }

        public string ApplicationName { get; set; } = nameof(ApplicationName);
        public IFileProvider ContentRootFileProvider { get; set; }
        public string ContentRootPath { get; set; }
        public string EnvironmentName { get; set; } = string.Empty;
    }
}
