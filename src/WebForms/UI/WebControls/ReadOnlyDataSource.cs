// MIT License.

using System.Collections;
using System.ComponentModel;
using System.Diagnostics;

namespace System.Web.UI.WebControls;
/// <devdoc>
/// Helper class for data bound controls to wrap an generic data source
/// with a strongly typed IDataSource. This class automatically handles
/// other IDataSources, IEnumerable, and IListSource objects.
/// This class uses ReadOnlyDataSourceView to represent individual views.
/// </devdoc>
internal sealed class ReadOnlyDataSource : IDataSource
{

    private static readonly string[] ViewNames = [];

    private readonly string _dataMember;
    private readonly object _dataSource;

    public ReadOnlyDataSource(object dataSource, string dataMember)
    {
        Debug.Assert(dataSource == null || (dataSource is IEnumerable || dataSource is IDataSource || dataSource is IListSource), "Expected dataSource to be either null, an IEnumerable, an IDataSource, or an IListSource.");
        _dataSource = dataSource;
        _dataMember = dataMember;
    }

    #region Implementation of IDataSource
    event EventHandler IDataSource.DataSourceChanged
    {
        add
        {
        }
        remove
        {
        }
    }

    /// <devdoc>
    /// Check for IDataSource, IListSource, and IEnumerable, and return an
    /// approprite DataSourceView.
    /// </devdoc>
    DataSourceView IDataSource.GetView(string viewName)
    {
        // Check first for IDataSource
        IDataSource ds = _dataSource as IDataSource;
        if (ds != null)
        {
            return ds.GetView(viewName);
        }

        IEnumerable enumerable = DataSourceHelper.GetResolvedDataSource(_dataSource, _dataMember);
        return new ReadOnlyDataSourceView(this, _dataMember, enumerable);
    }

    ICollection IDataSource.GetViewNames()
    {
        return ViewNames;
    }
    #endregion
}

