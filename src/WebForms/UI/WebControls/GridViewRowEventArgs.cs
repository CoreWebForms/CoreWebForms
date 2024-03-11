//MIT license

namespace System.Web.UI.WebControls;
/// <devdoc>
/// <para>Provides data for the <see langword='GridViewRow'/> event.</para>
/// </devdoc>
public class GridViewRowEventArgs : EventArgs {

    private readonly GridViewRow _row;

    /// <devdoc>
    /// <para>Initializes a new instance of <see cref='System.Web.UI.WebControls.GridViewRowEventArgs'/> class.</para>
    /// </devdoc>
    public GridViewRowEventArgs(GridViewRow row) {
        this._row = row;
    }

    /// <devdoc>
    /// <para>Gets an row in the <see cref='System.Web.UI.WebControls.GridView'/>. This property is read-only.</para>
    /// </devdoc>
    public GridViewRow Row {
        get {
            return _row;
        }
    }
}

