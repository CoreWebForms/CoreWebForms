// MIT License.

using System.Web.Routing;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace WebForms.Tests;

public class PageForRoutingTest : HostedTestBase
{
    [Fact]
    public async Task MapPageRouteTest()
    {
        //Arrange/Act
        var htmlResult = await RunPage<PageWithRoutingAPI>(services => services
            .AddSingleton<IStartupFilter>(new DelegateStartupFilter(app =>
            {
                app.ApplicationServices.GetRequiredService<RouteCollection>()
                    .MapPageRoute("ProductsByCategoryRoute", "Category/{categoryName}", "~/ProductList.aspx");
            })));

        Assert.Equal("<span id=\"/Category/MyTest\"></span>", htmlResult);
    }

    [Fact(Skip = "MapPageRoute is not enabled")]
    public async Task VerifyMappedRoute()
    {
        //Arrange/Act
        var htmlResult = await RunPage<PageWithRoutingAPI>(services => services
            .AddSingleton<IStartupFilter>(new DelegateStartupFilter(app =>
            {
                app.ApplicationServices.GetRequiredService<RouteCollection>()
                    .MapPageRoute("ProductsByCategoryRoute", "Category/{categoryName}", "~/ProductList.aspx");
            })), "/category/name");

        Assert.Equal("<span id=\"/Category/MyTest\"></span>", htmlResult);
    }

    private sealed class DelegateStartupFilter(Action<IApplicationBuilder> action) : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
            => builder =>
            {
                action(builder);
                next(builder);
            };
    }

    private sealed class PageWithRoutingAPI : Page
    {
        protected override void FrameworkInitialize()
        {
            var lbl = new Label
            {
                ID = GetRouteUrl("ProductsByCategoryRoute", new { categoryName = "MyTest" })
            };

            Controls.Add(lbl);
        }
    }
}
