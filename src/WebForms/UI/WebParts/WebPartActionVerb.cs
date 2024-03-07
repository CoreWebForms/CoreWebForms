// MIT License.

using System.ComponentModel;

namespace System.Web.UI.WebControls.WebParts
{
    internal abstract class WebPartActionVerb : WebPartVerb {

        [
        Bindable(false),
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        EditorBrowsable(EditorBrowsableState.Never),
        ]
        public override bool Checked {
            get {
                return false;
            }
            set {
                throw new NotSupportedException(SR.GetString(SR.WebPartActionVerb_CantSetChecked));
            }
        }

    }
}

