// MIT License.

namespace System.Web.UI;

internal interface IHtmlForm
{
    string ClientID
    {
        get;
    }

    string Method
    {
        get;
    }

    void RenderControl(HtmlTextWriter writer);

    void SetRenderMethodDelegate(RenderMethod renderMethod);
}
