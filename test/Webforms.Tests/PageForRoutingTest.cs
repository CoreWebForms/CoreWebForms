// MIT License.

using System.Web.Routing;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace WebForms.Tests;

[TestFixture]
public class PageForRoutingTest : HostedTestBase
{
    [Test]
    public async Task MapPageRouteTest()
    {
        //Arrange/Act
        var htmlResult = await RunPage<GetRouteUrlPage>(services => services
            .AddSingleton<IStartupFilter>(new DelegateStartupFilter(app =>
            {
                app.ApplicationServices.GetRequiredService<RouteCollection>()
                    .MapPageRoute("ProductsByCategoryRoute", "Category/{categoryName}", "~/");
            })));

        Assert.That(htmlResult, Is.EqualTo("<span id=\"/Category/MyTest\"></span>"));
    }

    [Test]
    public async Task VerifyMappedRoute()
    {
        //Arrange/Act
        var htmlResult = await RunPage<GetRouteValuePage>(services => services
            .AddSingleton<IStartupFilter>(new DelegateStartupFilter(app =>
            {
                app.ApplicationServices.GetRequiredService<RouteCollection>()
                    .MapPageRoute("ProductsByCategoryRoute", "Category/{categoryName}", "~/");
            })), "/category/mycategoryname");

        Assert.That(htmlResult, Is.EqualTo("<span id=\"mycategoryname\"></span>"));
    }

    [Test]
    public async Task UnmappedRoute()
    {
        //Arrange/Act
        var htmlResult = await RunPage<GetRouteValuePage>(services => services
            .AddSingleton<IStartupFilter>(new DelegateStartupFilter(app =>
            {
                app.ApplicationServices.GetRequiredService<RouteCollection>()
                    .MapPageRoute("ProductsByCategoryRoute", "Category/{categoryName}", "~/");
                app.ApplicationServices.GetRequiredService<RouteCollection>()
                    .MapPageRoute("ProductsByCategoryRoute2", "Category2/{categoryName}", "~/extra_route");
            })), "/category/mycategoryname");

        Assert.That(htmlResult, Is.EqualTo("<span id=\"mycategoryname\"></span>"));
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
