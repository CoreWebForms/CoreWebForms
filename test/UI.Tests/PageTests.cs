// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
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

    [Fact]
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
        await pipeline(httpContext).ConfigureAwait(false);

        body.Position = 0;

        using var reader = new StreamReader(body);
        return reader.ReadToEnd();
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
        private void Page_Load(object sender, EventArgs e)
        {
            Controls.Add(new LiteralControl("hello"));
        }
    }

    private sealed class Page4 : Page
    {
        private void Page_Load(object sender, EventArgs e)
        {
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

        public string? ApplicationName { get; set; } = nameof(ApplicationName);
        public IFileProvider ContentRootFileProvider { get; set; }
        public string ContentRootPath { get; set; }
        public string EnvironmentName { get; set; } = string.Empty;
    }
}
