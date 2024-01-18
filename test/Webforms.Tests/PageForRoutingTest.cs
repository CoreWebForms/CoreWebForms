// MIT License.

using System.Web.UI;
using System.Web.UI.WebControls;
using Xunit;

namespace WebForms.Tests;

[Collection(nameof(WebFormsBaseTest))]
public class PageForRoutingTest
{
    [Fact]
    public async Task MapPageRouteTest()
    {
        //Arrange and Act
        var htmlResult = await TestUtil.RunPage<PageWithRoutingAPI>(options =>
            {
                options.Routes.MapPageRoute("ProductsByCategoryRoute",
                    "Category/{categoryName}", "~/ProductList.aspx");
            });

        //Assert

        Assert.NotNull(htmlResult);
        Assert.True(htmlResult.Contains("Category/MyTest"), "Routing should convert based on MapPageRoute");
    }

    private sealed class PageWithRoutingAPI : Page
    {
        protected override void FrameworkInitialize()
        {
            Controls.Add(new LiteralControl("hello"));
            Label lbl = new Label();
            Controls.Add(lbl);
            Controls[1].ID = Controls[0].GetRouteUrl("ProductsByCategoryRoute", new { categoryName = "MyTest" });
        }
    }
}
