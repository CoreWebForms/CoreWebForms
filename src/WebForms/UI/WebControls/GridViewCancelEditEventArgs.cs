//MIT license

using System.ComponentModel;

namespace System.Web.UI.WebControls;
/// <devdoc>
/// <para>Provides data for some <see cref='System.Web.UI.WebControls.GridView'/> events.</para>
/// </devdoc>
public class GridViewCancelEditEventArgs : CancelEventArgs {

    private readonly int _rowIndex;

    /// <devdoc>
    /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.GridViewCancelEditEventArgs'/>
    /// class.</para>
    /// </devdoc>
    public GridViewCancelEditEventArgs(int rowIndex) {
        this._rowIndex = rowIndex;
    }

    public int RowIndex {
        get {
            return _rowIndex;
        }
    }
}

