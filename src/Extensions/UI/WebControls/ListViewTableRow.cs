// MIT License.

using System.Web.UI.HtmlControls;

namespace System.Web.UI.WebControls;

/// <summary>
/// Summary description for ListViewTableRow
/// </summary>
internal sealed class ListViewTableRow : HtmlTableRow
{
    public ListViewTableRow()
    {
    }

    protected override ControlCollection CreateControlCollection()
    {
        return new ControlCollection(this);
    }

    protected internal override void Render(HtmlTextWriter writer)
    {
        RenderChildren(writer);
    }
}
