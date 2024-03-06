// MIT License.

namespace System.Web.UI.WebControls.WebParts
{
    internal sealed class WebPartConnectionsConfigureVerb : WebPartActionVerb {

        // Properties must look at viewstate directly instead of the property in the base class,
        // so we can distinguish between an unset property and a property set to String.Empty.
        [
        WebSysDefaultValue(SR.WebPartConnectionsConfigureVerb_Description)
        ]
        public override string Description {
            get {
                object o = ViewState["Description"];
                return (o == null) ? SR.GetString(SR.WebPartConnectionsConfigureVerb_Description) : (string)o;
            }
            set {
                ViewState["Description"] = value;
            }
        }

        [
        WebSysDefaultValue(SR.WebPartConnectionsConfigureVerb_Text)
        ]
        public override string Text {
            get {
                object o = ViewState["Text"];
                return (o == null) ? SR.GetString(SR.WebPartConnectionsConfigureVerb_Text) : (string)o;
            }
            set {
                ViewState["Text"] = value;
            }
        }
    }
}
