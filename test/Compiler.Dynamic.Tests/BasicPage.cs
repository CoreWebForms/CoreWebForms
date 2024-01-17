// MIT License.

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
public class DynamicTests
{
    private readonly ITestOutputHelper _output;

    public DynamicTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [InlineData("test01", "basic_page.aspx")]
    [InlineData("test02", "code_behind.aspx")]
    [Theory]
    public async Task Test1(string test, string page)
    {
        // Arrange
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var contentRoot = Path.Combine(AppContext.BaseDirectory, "assets", test);
        var html = Path.Combine(contentRoot, $"{page}.html");
        var expected = File.Exists(html) ? File.ReadAllText(html) : string.Empty;

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
        string? result = null;

        do
        {
            using var response = await client.GetAsync(page, cts.Token).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
            else
            {
                await Task.Delay(250, cts.Token).ConfigureAwait(false);
            }
        } while (result is null);

        var tempPath = Path.Combine(Path.GetTempPath(), page);
        File.WriteAllText(tempPath, result);
        _output.WriteLine($"Wrote result to {tempPath}. To update test, run the following command:");
        _output.WriteLine($"cp {tempPath} {html}");

        Assert.Equal(expected, result);
    }
}
