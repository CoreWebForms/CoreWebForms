// MIT License.

using System.Collections;

namespace System.Web.UI.WebControls;
/// <devdoc>
/// This class is used by ReadOnlyDataSource to represent an individual
/// view of a generic data source.
/// </devdoc>
internal sealed class ReadOnlyDataSourceView : DataSourceView
{

    private readonly IEnumerable _dataSource;

    public ReadOnlyDataSourceView(ReadOnlyDataSource owner, string name, IEnumerable dataSource) : base(owner, name)
    {
        _dataSource = dataSource;
    }

    protected internal override IEnumerable ExecuteSelect(DataSourceSelectArguments arguments)
    {
        arguments.RaiseUnsupportedCapabilitiesError(this);
        return _dataSource;
    }
}

