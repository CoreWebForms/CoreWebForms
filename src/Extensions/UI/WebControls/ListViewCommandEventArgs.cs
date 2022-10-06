// MIT License.

using System.Diagnostics.CodeAnalysis;

namespace System.Web.UI.WebControls;

public class ListViewCommandEventArgs : CommandEventArgs
{
    private readonly ListViewItem _item;
    private readonly object _commandSource;

    [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "2#")]
    public ListViewCommandEventArgs(ListViewItem item, object commandSource, CommandEventArgs originalArgs) : base(originalArgs)
    {
        _item = item;
        _commandSource = commandSource;
    }

    public object CommandSource
    {
        get
        {
            return _commandSource;
        }
    }

    public ListViewItem Item
    {
        get
        {
            return _item;
        }
    }

    public bool Handled { get; set; }

}
