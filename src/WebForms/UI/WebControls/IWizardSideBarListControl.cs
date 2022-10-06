// MIT License.

namespace System.Web.UI.WebControls;

using System.Collections;

internal interface IWizardSideBarListControl
{
    object DataSource { get; set; }

    IEnumerable Items { get; }

    ITemplate ItemTemplate { get; set; }

    int SelectedIndex { get; set; }

    event CommandEventHandler ItemCommand;

    event EventHandler<WizardSideBarListControlItemEventArgs> ItemDataBound;

    void DataBind();
}
