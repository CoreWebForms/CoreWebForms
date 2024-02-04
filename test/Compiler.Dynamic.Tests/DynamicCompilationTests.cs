// MIT License.

using System.Diagnostics;
using System.Web.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Compiler.Dynamic.Tests;

[TestClass]
public class DynamicCompilationTests
{
    private static TestContext _context = null!;

    [ClassInitialize]
    public static void Initialize(TestContext context)
    {
        _context = context;
    }

    [DataTestMethod]
    [DataRow("test01", "basic_page.aspx")]
    [DataRow("test02", "code_behind.aspx")]
    [DataRow("test03", "page_with_master.aspx")]
    [DataRow("test04", "page_with_master.aspx", "other_page_with_master.aspx", "page_with_master.aspx")]
    [DataRow("test05", "error_page.aspx")]
    [DataRow("test06", "route_url_expressionbuilder.aspx")]
    public async Task CompiledPageRuns(string test, params string[] pages)
    {
        // Arrange
        using var cts = Debugger.IsAttached ? new CancellationTokenSource() : new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var contentProvider = new EmbeddedFileProvider(typeof(DynamicCompilationTests).Assembly, $"Compiler.Dynamic.Tests.assets.{test}");
        var expectedPages = pages
            .Select((page, index) => $"{page}._{index}.html")
            .Select(expectedHtmlPath =>
            {
                if (contentProvider.GetFileInfo(expectedHtmlPath) is { Exists: true } file)
                {
                    using var stream = file.CreateReadStream();
                    using var reader = new StreamReader(stream);

                    return reader.ReadToEnd();
                }

                return string.Empty;
            })
            .ToList();

        using var host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((ctx, _) =>
            {
                ctx.HostingEnvironment.ContentRootFileProvider = contentProvider;
            })
            .ConfigureWebHost(app =>
            {
                app.UseTestServer();
                app.Configure(app =>
                {
                    RouteTable.Routes.MapPageRoute("Test", "/test", pages[0]);

                    app.UseRouting();
                    app.UseSession();
                    app.UseSystemWebAdapters();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapHttpHandlers();
                    });
                });
                app.ConfigureServices(services =>
                {
                    services.AddDistributedMemoryCache();
                    services.AddRouting();
                    services.AddSystemWebAdapters()
                        .AddWrappedAspNetCoreSession()
                        .AddWebForms()
                        .AddDynamicPages();
                });
            })
            .Start();

        // Act
        var client = host.GetTestClient();

        for (int i = 0; i < pages.Length; i++)
        {
            var expectedHtml = expectedPages[i];
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
            _context.WriteLine($"Wrote result to {tempPath}");

            Assert.AreEqual(expectedHtml, result);
        }
    }

}
