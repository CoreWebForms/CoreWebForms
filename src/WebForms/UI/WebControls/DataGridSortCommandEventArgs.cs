//MIT license

using System.Diagnostics.CodeAnalysis;

namespace System.Web.UI.WebControls
{

    /// <devdoc>
    /// <para>Provides data for the <see langword='DataGridSortCommand'/> event of a <see cref='System.Web.UI.WebControls.DataGrid'/>.
    /// </para>
    /// </devdoc>
    public class DataGridSortCommandEventArgs : EventArgs {

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.DataGridSortCommandEventArgs'/> class.</para>
        /// </devdoc>
        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers")]
        public DataGridSortCommandEventArgs(object commandSource, DataGridCommandEventArgs dce) {
            this.CommandSource = commandSource;
            this.SortExpression = (string)dce.CommandArgument;
        }

        /// <devdoc>
        ///    <para>Gets the source of the command. This property is read-only. </para>
        /// </devdoc>
        public object CommandSource { get; }

        /// <devdoc>
        ///    <para>Gets the expression used to sort. This property is read-only.</para>
        /// </devdoc>
        public string SortExpression { get; }
    }
}

