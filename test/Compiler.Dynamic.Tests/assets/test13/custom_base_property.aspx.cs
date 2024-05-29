// MIT License.

using System.Web.UI;

public partial class PageWithBaseType : BasePage
{
}

public partial class BasePage : Page
{
    public string BasePageTitle { get; set; } = "My Base Page Title";
}
