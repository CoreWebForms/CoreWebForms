// MIT License.

namespace System.Web.UI.WebControls;

/// <devdoc>
/// <para>Represents an individual row in the <see cref='System.Web.UI.WebControls.FormView'/>.</para>
/// </devdoc>
public class FormViewPagerRow : FormViewRow, INonBindingContainer
{

    /// <devdoc>
    /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.FormViewRow'/> class.</para>
    /// </devdoc>
    public FormViewPagerRow(int rowIndex, DataControlRowType rowType, DataControlRowState rowState) : base(rowIndex, rowType, rowState)
    {
    }
}

