// MIT License.

using System.ComponentModel;

namespace System.Web.UI.WebControls;

public class ListViewEditEventArgs : CancelEventArgs
{
    private readonly int _newEditIndex;

    public ListViewEditEventArgs(int newEditIndex) : base(false)
    {
        _newEditIndex = newEditIndex;
    }

    /// <devdoc>
    /// <para>Gets the int argument to the command posted to the <see cref='System.Web.UI.WebControls.ListView'/>. This property is read-only.</para>
    /// </devdoc>
    public int NewEditIndex
    {
        get
        {
            return _newEditIndex;
        }
    }
}
