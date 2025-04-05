// MIT License.

using System.Collections;

namespace System.Web.UI.Design;

public abstract class DesignerDataSourceView
{
    protected DesignerDataSourceView(IDataSourceDesigner owner, string viewName) { }

    public virtual bool CanDelete { get; }
    public virtual bool CanInsert { get; }
    public virtual bool CanPage { get; }
    public virtual bool CanRetrieveTotalRowCount { get; }
    public virtual bool CanSort { get; }
    public virtual bool CanUpdate { get; }
    public IDataSourceDesigner DataSourceDesigner { get; }
    public string Name { get; }
    public virtual IDataSourceViewSchema Schema { get; }

    public virtual IEnumerable GetDesignTimeData(int minimumRows, out bool isSampleData) => throw new NotImplementedException();
}
