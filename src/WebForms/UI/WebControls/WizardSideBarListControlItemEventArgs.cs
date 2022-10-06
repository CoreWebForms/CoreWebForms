// MIT License.

namespace System.Web.UI.WebControls;

internal sealed class WizardSideBarListControlItemEventArgs : EventArgs
{
#if PORT_WIZARD
    public WizardSideBarListControlItem Item
    {
        get;
        private set;
    }

    public WizardSideBarListControlItemEventArgs(WizardSideBarListControlItem item)
    {
        Item = item;
    }
#endif
}
