// MIT License.

using System.Diagnostics.CodeAnalysis;

namespace System.Web.UI.WebControls;

/// <devdoc>
/// <para>Provides data for some <see cref='System.Web.UI.WebControls.DataGrid'/> events.</para>
/// </devdoc>
public class DataGridCommandEventArgs : CommandEventArgs
{

    private readonly DataGridItem item;
    private readonly object commandSource;

    /// <devdoc>
    /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.DataGridCommandEventArgs'/>
    /// class.</para>
    /// </devdoc>
    [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers")]
    public DataGridCommandEventArgs(DataGridItem item, object commandSource, CommandEventArgs originalArgs) : base(originalArgs)
    {
        this.item = item;
        this.commandSource = commandSource;
    }

    /// <devdoc>
    ///    <para>Gets the source of the command. This property is read-only.</para>
    /// </devdoc>
    public object CommandSource
    {
        get
        {
            return commandSource;
        }
    }

    /// <devdoc>
    /// <para>Gets the item in the <see cref='System.Web.UI.WebControls.DataGrid'/> that was clicked. This property is read-only.</para>
    /// </devdoc>
    public DataGridItem Item
    {
        get
        {
            return item;
        }
    }
}

