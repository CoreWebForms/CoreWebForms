// MIT License.

using System.Web.UI;
using Microsoft.AspNetCore.SystemWebAdapters.UI;

namespace SystemWebUISample.Pages;

[AspxPage("/Test.aspx")]
public sealed class TestPage : Page
{
    private void Page_Load(object sender, EventArgs e)
    {
        Controls.Add(new LiteralControl("hello"));
    }
}
