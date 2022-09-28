// MIT License.

namespace System.Web.UI.WebControls;

/// <devdoc>
///    <para> Filters and retrieves several types of values from validated controls.</para>
/// </devdoc>
public class AssociatedControlConverter : ControlIDConverter
{
    /// <devdoc>
    ///    <para>Determines whether a given control should have its id added to the StandardValuesCollection.</para>
    /// </devdoc>
    protected override bool FilterControl(Control control)
    {
        return control is WebControl;
    }
}
