// MIT License.

namespace System.Web.UI;

public interface IHierarchicalDataSource
{

    // events
    event EventHandler DataSourceChanged;

    // methods
    HierarchicalDataSourceView GetHierarchicalView(string viewPath);
}

