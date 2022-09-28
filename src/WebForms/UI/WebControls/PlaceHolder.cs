// MIT License.

using System.ComponentModel;

namespace System.Web.UI.WebControls;

public class PlaceHolder : Control
{

    /// <devdoc>
    ///    <para>Gets and sets a value indicating whether theme is enabled.</para>
    /// </devdoc>
    [
    Browsable(true)
    ]
    public override bool EnableTheming
    {
        get
        {
            return base.EnableTheming;
        }
        set
        {
            base.EnableTheming = value;
        }
    }
}
