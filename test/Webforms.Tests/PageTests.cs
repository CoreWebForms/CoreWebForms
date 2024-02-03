// MIT License.

using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebForms.Tests;

[TestClass]
public class PageTests : HostedTestBase
{
    [TestMethod]
    public async Task EmptyPage()
    {
        // Arrange/Act
        var result = await RunPage<Page1>();

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    public async Task CustomRender()
    {
        // Arrange/Act
        var result = await RunPage<Page2>();

        // Assert
        Assert.AreEqual("hello", result);
    }

    [TestMethod]
    public async Task PageLoadAddControl()
    {
        // Arrange/Act
        var result = await RunPage<Page3>();

        // Assert
        Assert.AreEqual("hello", result);
    }

    [TestMethod]
    [Ignore("Currently not working")]
    public async Task PageWithForm()
    {
        // Arrange/Act
        var result = await RunPage<Page4>();

        // Assert
        Assert.AreEqual("<form method=\"post\" action=\"/path\"><div class=\"aspNetHidden\"</div></form>", result);
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
        protected override void FrameworkInitialize()
        {
            Controls.Add(new LiteralControl("hello"));
        }
    }

    private sealed class Page4 : Page
    {
        protected override void FrameworkInitialize()
        {
            base.FrameworkInitialize();

            var form = new HtmlForm();
            form.Controls.Add(new TextBox());

            Controls.Add(form);
        }
    }

}
