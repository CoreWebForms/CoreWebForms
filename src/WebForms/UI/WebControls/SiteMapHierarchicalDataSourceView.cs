//MIT license

namespace System.Web.UI.WebControls;

public class SiteMapHierarchicalDataSourceView : HierarchicalDataSourceView
{

    private readonly SiteMapNodeCollection _collection;

    public SiteMapHierarchicalDataSourceView(SiteMapNode node)
    {
        _collection = new SiteMapNodeCollection(node);
    }

    public SiteMapHierarchicalDataSourceView(SiteMapNodeCollection collection)
    {
        _collection = collection;
    }

    public override IHierarchicalEnumerable Select()
    {
        return _collection;
    }
}
