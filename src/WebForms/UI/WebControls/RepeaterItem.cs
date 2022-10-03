// MIT License.

using System.ComponentModel;

namespace System.Web.UI.WebControls;
/// <devdoc>
/// <para>Encapsulates an item within the <see cref='System.Web.UI.WebControls.Repeater'/> control.</para>
/// </devdoc>
[
ToolboxItem(false)
]
public class RepeaterItem : Control, IDataItemContainer
{

    private readonly int itemIndex;
    private readonly ListItemType itemType;
    private object dataItem;

    /// <devdoc>
    /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.RepeaterItem'/> with the specified item type and
    ///    location.</para>
    /// </devdoc>
    public RepeaterItem(int itemIndex, ListItemType itemType)
    {
        this.itemIndex = itemIndex;
        this.itemType = itemType;
    }

    /// <devdoc>
    ///    Specifies the data item.
    /// </devdoc>
    public virtual object DataItem
    {
        get
        {
            return dataItem;
        }
        set
        {
            dataItem = value;
        }
    }

    /// <devdoc>
    ///    <para>Indicates the ordinal index that specifies the item 
    ///       location within the <see cref='System.Web.UI.WebControls.Repeater'/>
    ///       .</para>
    /// </devdoc>
    public virtual int ItemIndex
    {
        get
        {
            return itemIndex;
        }
    }

    /// <devdoc>
    ///    Indicates the <see cref='System.Web.UI.WebControls.Repeater'/> item type. This property is
    ///    read-only.
    /// </devdoc>
    public virtual ListItemType ItemType
    {
        get
        {
            return itemType;
        }
    }

    /// <internalonly/>
    /// <devdoc>
    /// </devdoc>
    protected override bool OnBubbleEvent(object source, EventArgs e)
    {
        if (e is CommandEventArgs)
        {
            RepeaterCommandEventArgs args = new RepeaterCommandEventArgs(this, source, (CommandEventArgs)e);

            RaiseBubbleEvent(this, args);
            return true;
        }
        return false;
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

