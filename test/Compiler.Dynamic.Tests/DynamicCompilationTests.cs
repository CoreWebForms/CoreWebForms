// MIT License.

using System.Diagnostics;
using System.Net;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebForms.Features;

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
    [DataRow("test01", "basic_page.aspx", "mapped-page")]
    [DataRow("test02", "code_behind.aspx")]
    [DataRow("test03", "page_with_master.aspx")]
    [DataRow("test04", "page_with_master.aspx", "other_page_with_master.aspx", "page_with_master.aspx")]
    [DataRow("test05", "error_page.aspx")]
    [DataRow("test06", "route_url_expressionbuilder.aspx")]
    [DataRow("test07", "redirect_page.aspx")]
    [DataRow("test08", "scripts.aspx")]
    [DataRow("test09", "basic_page_with_usercontrol.aspx")]
    [DataRow("test10", "loadusercontrol.aspx")]
    [DataRow("test11", "cspage.aspx")]
    [DataRow("test12", "folder/subfolder.aspx")]
    public async Task CompiledPageRuns(string test, params string[] pages)
    {
        if (test == "test08")
        {
            Assert.Inconclusive("Currently broken on CI");
        }

        // Arrange
        using var cts = Debugger.IsAttached ? new CancellationTokenSource() : new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var contentProvider = new PhysicalFileProvider(Path.Combine(AppContext.BaseDirectory, "assets", test));
        var expectedPages = pages
            .Select((page, index) => $"{NormalizePage(page)}._{index}.html")
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
                    app.UseRouting();
                    app.UseSession();
                    app.UseSystemWebAdapters();
                    app.Use((ctx, next) =>
                    {
                        if (ctx.GetEndpoint() is { })
                        {
                            Assert.IsNotNull(ctx.Features.Get<IWebFormsCompilationFeature>());
                        }

                        return next(ctx);
                    });
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapHttpHandlers();
                    });
                });
                app.ConfigureServices(services =>
                {
                    services.ConfigureHttpJsonOptions(options =>
                    {
                        options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
                        options.SerializerOptions.WriteIndented = true;
                    });

                    services.AddDistributedMemoryCache();
                    services.AddRouting();
                    services.AddSystemWebAdapters()
                        .AddWrappedAspNetCoreSession()
                        .AddRouting(routes =>
                        {
                            routes.MapPageRoute("MappedPage", "/mapped-page", pages[0]);

                            // for route builder test
                            routes.MapPageRoute("Test", "/test", pages[0]);
                        })
                        .AddWebForms()
                        .AddScriptManager()
                        .AddDynamicPages();
                    services.AddSingleton<IDataProtectionProvider, NoopDataProtector>();
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
            var currentPage = page;

            do
            {
                using var response = await client.GetAsync(currentPage, cts.Token);

                if (response.StatusCode == HttpStatusCode.Found)
                {
                    currentPage = response.Headers.Location!.ToString();
                }
                else if (response.IsSuccessStatusCode)
                {
                    result = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    await Task.Delay(250, cts.Token);
                }
            } while (result is null);

            var tempPath = Path.Combine(Path.GetTempPath(), $"{NormalizePage(page)}._{i}.html");
            File.WriteAllText(tempPath, result);
            _context.WriteLine($"Wrote result to {tempPath}");

            Assert.AreEqual(expectedHtml.ReplaceLineEndings(), result.ReplaceLineEndings());
        }
    }

    private static string NormalizePage(string path) => path.Replace("/", "__");

    // Allows for data protection to be turned off for testing purposes.
    private sealed class NoopDataProtector : IDataProtector, IDataProtectionProvider
    {
        public IDataProtector CreateProtector(string purpose) => this;

        public byte[] Protect(byte[] plaintext) => plaintext;

        public byte[] Unprotect(byte[] protectedData) => protectedData;
    }
}
