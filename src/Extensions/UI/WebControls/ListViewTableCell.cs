// MIT License.

using System.Web.UI.HtmlControls;

namespace System.Web.UI.WebControls;

/// <summary>
/// Summary description for ListViewTableCell
/// </summary>
internal sealed class ListViewTableCell : HtmlTableCell
{
    public ListViewTableCell()
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
