// MIT License.

using System.ComponentModel;

namespace System.Web.UI.WebControls;

public class ListViewCancelEventArgs : CancelEventArgs
{
    private readonly int _itemIndex;
    private readonly ListViewCancelMode _cancelMode;

    public ListViewCancelEventArgs(int itemIndex, ListViewCancelMode cancelMode) : base(false)
    {
        _itemIndex = itemIndex;
        _cancelMode = cancelMode;
    }

    public int ItemIndex
    {
        get
        {
            return _itemIndex;
        }
    }

    public ListViewCancelMode CancelMode
    {
        get
        {
            return _cancelMode;
        }
    }
}
