//MIT license

using System.Diagnostics.CodeAnalysis;

namespace System.Web.UI.WebControls;

/// <devdoc>
/// <para>Provides data for some <see cref='System.Web.UI.WebControls.GridView'/> events.</para>
/// </devdoc>
public class GridViewCommandEventArgs : CommandEventArgs {

    private readonly GridViewRow _row;
    private readonly object _commandSource;
    

    /// <devdoc>
    /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.GridViewCommandEventArgs'/>
    /// class.</para>
    /// </devdoc>
    public GridViewCommandEventArgs(GridViewRow row, object commandSource, CommandEventArgs originalArgs) : base(originalArgs) {
        this._row = row;
        this._commandSource = commandSource;
    }

    /// <devdoc>
    /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.GridViewCommandEventArgs'/>
    /// class.</para>
    /// </devdoc>
    [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers")]
    public GridViewCommandEventArgs(object commandSource, CommandEventArgs originalArgs) : base(originalArgs) {
        this._commandSource = commandSource;
    }

    /// <devdoc>
    ///    <para>Gets the source of the command. This property is read-only.</para>
    /// </devdoc>
    public object CommandSource {
        get {
            return _commandSource;
        }
    }

    /// <summary>
    /// Set by the user to skip databound or datasource handling of the event.
    /// </summary>
    public bool Handled { get; set; }

    /// <devdoc>
    /// <para>Gets the row in the <see cref='System.Web.UI.WebControls.GridView'/> that was clicked. This property is read-only.</para>
    /// </devdoc>
    internal GridViewRow Row {
        get {
            return _row;
        }
    }
}

