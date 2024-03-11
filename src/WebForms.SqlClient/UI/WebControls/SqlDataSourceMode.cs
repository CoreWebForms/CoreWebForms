// MIT License.

namespace System.Web.UI.WebControls;
/// <devdoc>
/// Specifies the behavior of the SqlDataSource.
/// </devdoc>
public enum SqlDataSourceMode
{

    /// <devdoc>
    /// The SqlDataSource uses a DataReader, which does not allow sorting or paging.
    /// </devdoc>
    DataReader = 0,

    /// <devdoc>
    /// The SqlDataSource uses a DataSet, which allows sorting and paging.
    /// </devdoc>
    DataSet = 1,
}

