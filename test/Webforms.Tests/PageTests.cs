// MIT License.

using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using NUnit.Framework;

namespace WebForms.Tests;

[TestFixture]
public class PageTests : HostedTestBase
{
    [Test]
    public async Task EmptyPage()
    {
        // Arrange/Act
        var result = await RunPage<Page1>();

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task CustomRender()
    {
        // Arrange/Act
        var result = await RunPage<Page2>();

        // Assert
        Assert.That(result, Is.EqualTo("hello"));
    }

    [Test]
    public async Task PageLoadAddControl()
    {
        // Arrange/Act
        var result = await RunPage<Page3>();

        // Assert
        Assert.That(result, Is.EqualTo("hello"));
    }

    [Test]
    [Ignore("Currently not working")]
    public async Task PageWithForm()
    {
        // Arrange/Act
        var result = await RunPage<Page4>();

        // Assert
        Assert.That(result, Is.EqualTo("<form method=\"post\" action=\"/path\"><div class=\"aspNetHidden\"</div></form>"));
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
