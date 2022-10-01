// MIT License.

namespace System.Web.UI.WebControls;

using System.Web.UI;

/// <devdoc>
/// This class is used by ReadOnlyHierarchicalDataSource to represent an
/// individual view of a generic hierarchical data source.
/// </devdoc>
internal sealed class ReadOnlyHierarchicalDataSourceView : HierarchicalDataSourceView
{

    private readonly IHierarchicalEnumerable _dataSource;

    public ReadOnlyHierarchicalDataSourceView(IHierarchicalEnumerable dataSource)
    {
        _dataSource = dataSource;
    }

    public override IHierarchicalEnumerable Select()
    {
        return _dataSource;
    }
}

