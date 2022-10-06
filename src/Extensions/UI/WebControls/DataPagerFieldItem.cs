// MIT License.

using System.Diagnostics.CodeAnalysis;

namespace System.Web.UI.WebControls;

// This class implements INonBindingContainer to allow binding statements on TemplatePagerField
// to look like Container.TotalRowCount rather than Container.Pager.TotalRowCount.
public class DataPagerFieldItem : Control, INonBindingContainer
{
    private readonly DataPagerField _field;
    private readonly DataPager _pager;

    public DataPagerFieldItem(DataPagerField field, DataPager pager)
    {
        _field = field;
        _pager = pager;
    }

    public DataPager Pager
    {
        get
        {
            return _pager;
        }
    }

    public DataPagerField PagerField
    {
        get
        {
            return _field;
        }
    }

    [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "1#")]
    protected override bool OnBubbleEvent(object source, EventArgs e)
    {
        if (e is CommandEventArgs)
        {
            DataPagerFieldCommandEventArgs args = new DataPagerFieldCommandEventArgs(this, source, (CommandEventArgs)e);
            RaiseBubbleEvent(this, args);
            return true;
        }
        return false;
    }
}
