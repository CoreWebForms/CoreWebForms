//MIT license

using System.ComponentModel;

namespace System.Web.UI.WebControls;

/// <devdoc>
///    <para>Provides data for 
///       the <see langword='GridViewPage'/>
///       event.</para>
/// </devdoc>
public class GridViewPageEventArgs : CancelEventArgs {

    private int _newPageIndex;

    /// <devdoc>
    /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.GridViewPageEventArgs'/> class.</para>
    /// </devdoc>
    public GridViewPageEventArgs(int newPageIndex) {
        this._newPageIndex = newPageIndex;
    }

    /// <devdoc>
    /// <para>Gets the index of the first new page to be displayed in the <see cref='System.Web.UI.WebControls.GridView'/>. 
    ///    This property is read-only.</para>
    /// </devdoc>
    public int NewPageIndex {
        get {
            return _newPageIndex;
        }
        set {
            if (value < 0) {
                throw new ArgumentOutOfRangeException(nameof(value));
            }
            _newPageIndex = value;
        }
    }
}

