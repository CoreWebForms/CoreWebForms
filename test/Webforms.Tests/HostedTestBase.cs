// MIT License.

using System.Web;
using System.Web.UI;
using Compiler.Dynamic.Tests;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace WebForms.Tests;

[Collection(nameof(SelfHostedTests))]
public abstract class HostedTestBase
{
    protected async Task<string> RunPage<TPage>(Action<IServiceCollection>? servicesConfigure = null, string? path=null)
        where TPage : Page, new()
    {
        path ??= "/";
        using var host = await Host.CreateDefaultBuilder()
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
                        endpoints.MapHttpHandler<TPage>("/");
                    });
                });
                app.ConfigureServices(services =>
                {
                    services.AddDistributedMemoryCache();
                    services.AddRouting();
                    services.AddSystemWebAdapters()
                        .AddWrappedAspNetCoreSession()
                        .AddWebForms();

                    servicesConfigure?.Invoke(services);

                });
            })
            .StartAsync();

        using var client = host.GetTestClient();

        return await client.GetStringAsync(path);
    }
}
