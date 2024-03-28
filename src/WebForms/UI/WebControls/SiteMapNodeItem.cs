//MIT license

using System.ComponentModel;

namespace System.Web.UI.WebControls;

/// <devdoc>
/// <para>Represents a <see cref='System.Web.SiteMapNode'/></para>
/// </devdoc>
[
ToolboxItem(false)
]
public class SiteMapNodeItem : WebControl, INamingContainer, IDataItemContainer
{

    private readonly int _itemIndex;
    private SiteMapNodeItemType _itemType;
    private SiteMapNode _siteMapNode;

    /// <devdoc>
    /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.SiteMapNodeItem'/> class.</para>
    /// </devdoc>
    public SiteMapNodeItem(int itemIndex, SiteMapNodeItemType itemType)
    {
        _itemIndex = itemIndex;
        _itemType = itemType;
    }

    /// <devdoc>
    /// <para>Represents a sitemapnode. </para>
    /// </devdoc>
    public virtual SiteMapNode SiteMapNode
    {
        get
        {
            return _siteMapNode;
        }
        set
        {
            _siteMapNode = value;
        }
    }

    /// <devdoc>
    /// <para>Indicates the index of the item. This property is read-only.</para>
    /// </devdoc>
    public virtual int ItemIndex
    {
        get
        {
            return _itemIndex;
        }
    }

    /// <devdoc>
    /// <para>Indicates the type of the item in the <see cref='System.Web.UI.WebControls.DataList'/>.</para>
    /// </devdoc>
    public virtual SiteMapNodeItemType ItemType
    {
        get
        {
            return _itemType;
        }
    }

    /// <internalonly/>
    /// <devdoc>
    /// </devdoc>
    protected internal virtual void SetItemType(SiteMapNodeItemType itemType)
    {
        _itemType = itemType;
    }

    /// <internalonly/>
    object IDataItemContainer.DataItem
    {
        get
        {
            return SiteMapNode;
        }
    }

    int IDataItemContainer.DataItemIndex
    {
        get
        {
            return ItemIndex;
        }
    }

    int IDataItemContainer.DisplayIndex
    {
        get
        {
            return ItemIndex;
        }
    }
}
