// MIT License.

using System.ComponentModel;

namespace System.Web.UI.WebControls;

/// <devdoc>
/// <para> Defines the properties and methods of the <see cref='System.Web.UI.WebControls.TableSectionStyle'/> class.</para>
/// </devdoc>
public class TableSectionStyle : Style
{

    /// <devdoc>
    ///    <para>
    ///        Gets or sets the visibility of the table section.
    ///    </para>
    /// </devdoc>
    [
    WebCategory("Behavior"),
    DefaultValue(true),
    WebSysDescription(SR.TableSectionStyle_Visible),
    NotifyParentProperty(true)
    ]
    public bool Visible
    {
        get
        {
            object visible = ViewState["Visible"];
            return ((visible == null) ? true : (bool)visible);
        }
        set
        {
            ViewState["Visible"] = value;
        }
    }
}

