// MIT License.

namespace System.Web.UI.Design;

public interface IControlDesigner
{
    bool Visible { get; }
    string CreatePlaceHolderDesignTimeHtml();
    void UpdateDesignTimeHtml();
}
