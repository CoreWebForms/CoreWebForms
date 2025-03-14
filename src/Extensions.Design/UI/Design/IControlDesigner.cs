// MIT License.

namespace System.Web.UI.Design;

internal interface IControlDesigner
{
    bool Visible { get; }
    string CreatePlaceHolderDesignTimeHtml();
    void UpdateDesignTimeHtml();
}
