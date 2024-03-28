//MIT license

namespace System.Web.UI.WebControls;

public class SiteMapNodeItemEventArgs : EventArgs
{

    private readonly SiteMapNodeItem _item;

    /// <devdoc>
    /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.SiteMapNodeItemEventArgs'/> class.</para>
    /// </devdoc>
    public SiteMapNodeItemEventArgs(SiteMapNodeItem item)
    {
        this._item = item;
    }

    /// <devdoc>
    /// <para> Gets the <see cref='System.Web.UI.WebControls.SiteMapNodeItem'/> associated with the event.</para>
    /// </devdoc>
    public SiteMapNodeItem Item
    {
        get
        {
            return _item;
        }
    }
}
