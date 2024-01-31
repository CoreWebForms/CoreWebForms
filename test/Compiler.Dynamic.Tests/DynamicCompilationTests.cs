// MIT License.

using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Xunit;
using Xunit.Abstractions;

namespace Compiler.Dynamic.Tests;

[Collection(nameof(SelfHostedTests))]
public class DynamicCompilationTests
{
    private readonly ITestOutputHelper _output;

    public DynamicCompilationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [InlineData("test01", "basic_page.aspx")]
    [InlineData("test02", "code_behind.aspx")]
    [InlineData("test03", "page_with_master.aspx")]
    [InlineData("test04", "page_with_master.aspx", "other_page_with_master.aspx", "page_with_master.aspx")]
    [InlineData("test05", "error_page.aspx")]
    [Theory]
    public async Task CompiledPageRuns(string test, params string[] pages)
    {
        // Arrange
        using var cts = Debugger.IsAttached ? new CancellationTokenSource() : new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var contentRoot = Path.Combine(AppContext.BaseDirectory, "assets", test);
        var expectedHtmls = pages
            .Select((page, index) => Path.Combine(contentRoot, $"{page}._{index}.html"))
            .Select(expectedHtmlPath => File.Exists(expectedHtmlPath) ? File.ReadAllText(expectedHtmlPath) : string.Empty).ToArray();

        using var contentProvider = new PhysicalFileProvider(contentRoot);
        using var host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((ctx, _) =>
            {
                ctx.HostingEnvironment.ContentRootPath = contentRoot;
                ctx.HostingEnvironment.ContentRootFileProvider = contentProvider;
            })
            .ConfigureWebHost(app =>
            {
                app.UseTestServer();
                app.Configure(app =>
                {
                    app.UseRouting();
                    app.UseSession();
                    app.UseSystemWebAdapters();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapWebForms();
                    });
                });
                app.ConfigureServices(services =>
                {
                    services.AddDistributedMemoryCache();
                    services.AddRouting();
                    services.AddSystemWebAdapters()
                        .AddWrappedAspNetCoreSession()
                        .AddWebForms()
                        .AddDynamicWebForms();

                    services.AddOptions<SystemWebAdaptersOptions>().Configure(options =>
                    {
                        options.AppDomainAppPath = contentRoot;
                    });
                });
            })
            .Start();

        // Act
        var client = host.GetTestClient();

        for (int i = 0; i < pages.Length; i++)
        {
            var expectedHtml = expectedHtmls[i];
            var page = pages[i];

            string? result = null;

            do
            {
                using var response = await client.GetAsync(page, cts.Token);

                if (response.IsSuccessStatusCode)
                {
                    result = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    await Task.Delay(250, cts.Token);
                }
            } while (result is null);

            var tempPath = Path.Combine(Path.GetTempPath(), $"{page}._{i}.html");
            File.WriteAllText(tempPath, result);
            _output.WriteLine($"Wrote result to {tempPath}");

            Assert.Equal(expectedHtml, result);
        }
    }
}
