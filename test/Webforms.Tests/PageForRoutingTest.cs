// MIT License.

using System.Web.Routing;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebForms.Tests;

[TestClass]
public class PageForRoutingTest : HostedTestBase
{
    [TestMethod]
    public async Task MapPageRouteTest()
    {
        //Arrange/Act
        var htmlResult = await RunPage<GetRouteUrlPage>(services => services
            .AddSingleton<IStartupFilter>(new DelegateStartupFilter(app =>
            {
                app.ApplicationServices.GetRequiredService<RouteCollection>()
                    .MapPageRoute("ProductsByCategoryRoute", "Category/{categoryName}", "~/");
            })));

        Assert.AreEqual("<span id=\"/Category/MyTest\"></span>", htmlResult);
    }

    [TestMethod]
    public async Task VerifyMappedRoute()
    {
        //Arrange/Act
        var htmlResult = await RunPage<GetRouteValuePage>(services => services
            .AddSingleton<IStartupFilter>(new DelegateStartupFilter(app =>
            {
                app.ApplicationServices.GetRequiredService<RouteCollection>()
                    .MapPageRoute("ProductsByCategoryRoute", "Category/{categoryName}", "~/");
            })), "/category/mycategoryname");

        Assert.AreEqual("<span id=\"mycategoryname\"></span>", htmlResult);
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

    private sealed class GetRouteUrlPage : Page
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

    private sealed class GetRouteValuePage : Page
    {
        protected override void FrameworkInitialize()
        {
            var lbl = new Label
            {
                ID = RouteData.Values["categoryName"]!.ToString()
            };

            Controls.Add(lbl);
        }
    }
}
