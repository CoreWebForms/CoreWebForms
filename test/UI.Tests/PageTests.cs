using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using System.Text;
using System.Web;
using System.Web.UI;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.UI.Tests;

public class PageTests
{
    [Fact]
    public async Task EmptyPage()
    {
        // Arrange/Act
        var result = await RunPage<Page1>();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task CustomRender()
    {
        // Arrange/Act
        var result = await RunPage<Page2>();

        // Assert
        Assert.Equal("hello", result);
    }

    private async Task<string> RunPage<TPage>()
        where TPage : Page
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddSingleton<IHostEnvironment>(new Env());
        services.AddSingleton(new DiagnosticListener(Guid.NewGuid().ToString()));
        services.AddLogging();
        services.AddOptions();
        services.AddRouting();
        services.AddSystemWebAdapters();

        using var provider = services.BuildServiceProvider();

        var app = new ApplicationBuilder(provider);

        app.UseRouting();
        app.UseSystemWebAdapters();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapPage<TPage>("/path");
        });

        var pipeline = app.Build();
        var body = new MemoryStream();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/path";
        httpContext.Response.Body = body;

        // Act
        await pipeline(httpContext);

        body.Position = 0;

        using var reader = new StreamReader(body);
        return reader.ReadToEnd();
    }

    private class Page1 : Page
    {
    }

    private class Page2 : Page
    {
        protected override void Render(HtmlTextWriter writer)
        {
            writer.Write("hello");
        }
    }

    private class Env : IHostEnvironment
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