// MIT License.

using System.Diagnostics;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
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
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.UI.Tests;

public class PageTests
{
    [Fact]
    public async Task EmptyPage()
    {
        // Arrange/Act
        var result = await RunPage<Page1>().ConfigureAwait(false);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task CustomRender()
    {
        // Arrange/Act
        var result = await RunPage<Page2>().ConfigureAwait(false);

        // Assert
        Assert.Equal("hello", result);
    }

    [Fact]
    public async Task PageLoadAddControl()
    {
        // Arrange/Act
        var result = await RunPage<Page3>().ConfigureAwait(false);

        // Assert
        Assert.Equal("hello", result);
    }

    [Fact(Skip = "Not working")]
    public async Task PageWithForm()
    {
        // Arrange/Act
        var result = await RunPage<Page4>().ConfigureAwait(false);

        // Assert
        Assert.Equal("<form method=\"post\" action=\"/path\"><div class=\"aspNetHidden\"</div></form>", result);
    }

    private static async Task<string> RunPage<TPage>()
        where TPage : Page
    {
        // Arrange
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
                options.Routes.Add<TPage>("/path");
            });

        using var provider = services.BuildServiceProvider();

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
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/path";
        httpContext.Response.Body = body;
        httpContext.RequestServices = provider;

        // Act
        await pipeline(httpContext).ConfigureAwait(false);

        body.Position = 0;

        using var reader = new StreamReader(body);
        return reader.ReadToEnd();
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

    private sealed class Page1 : Page
    {
    }

    private sealed class Page2 : Page
    {
        protected override void Render(HtmlTextWriter writer)
        {
            writer.Write("hello");
        }
    }

    private sealed class Page3 : Page
    {
        protected override void FrameworkInitialize()
        {
            Controls.Add(new LiteralControl("hello"));
        }
    }

    private sealed class Page4 : Page
    {
        protected override void FrameworkInitialize()
        {
            base.FrameworkInitialize();

            var form = new HtmlForm();
            form.Controls.Add(new TextBox());

            Controls.Add(form);
        }
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
