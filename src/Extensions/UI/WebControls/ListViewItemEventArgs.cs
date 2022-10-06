// MIT License.

namespace System.Web.UI.WebControls;

/// <summary>
/// Summary description for ListViewItemEventArgs
/// </summary>
public class ListViewItemEventArgs : EventArgs
{
    private readonly ListViewItem _item;

    public ListViewItemEventArgs(ListViewItem item)
    {
        _item = item;
    }

    public ListViewItem Item
    {
        get
        {
            return _item;
        }
    }
}
