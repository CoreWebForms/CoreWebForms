// MIT License.

namespace System.Web.UI.WebControls;

using System.ComponentModel;

/// <devdoc>
///    <para> Filters and retrieves several types of values from validated controls.</para>
/// </devdoc>
public class ValidatedControlConverter : ControlIDConverter
{

    /// <devdoc>
    ///    <para>Determines whether a given control should have its id added to the StandardValuesCollection.</para>
    /// </devdoc>
    protected override bool FilterControl(Control control)
    {
        ValidationPropertyAttribute valProp = (ValidationPropertyAttribute)TypeDescriptor.GetAttributes(control)[typeof(ValidationPropertyAttribute)];
        return valProp != null && valProp.Name != null;
    }
}
