// MIT License.

namespace System.Web.UI.WebControls.WebParts
{
    internal sealed class WebPartMinimizeVerb : WebPartActionVerb {

        private string _defaultDescription;
        private string _defaultText;

        private string DefaultDescription {
            get {
                if (_defaultDescription == null) {
                    _defaultDescription = SR.GetString(SR.WebPartMinimizeVerb_Description);
                }
                return _defaultDescription;
            }
        }

        private string DefaultText {
            get {
                if (_defaultText == null) {
                    _defaultText = SR.GetString(SR.WebPartMinimizeVerb_Text);
                }
                return _defaultText;
            }
        }

        // Properties must look at viewstate directly instead of the property in the base class,
        // so we can distinguish between an unset property and a property set to String.Empty.
        [
        WebSysDefaultValue(SR.WebPartMinimizeVerb_Description)
        ]
        public override string Description {
            get {
                object o = ViewState["Description"];
                return (o == null) ? DefaultDescription : (string)o;
            }
            set {
                ViewState["Description"] = value;
            }
        }

        [
        WebSysDefaultValue(SR.WebPartMinimizeVerb_Text)
        ]
        public override string Text {
            get {
                object o = ViewState["Text"];
                return (o == null) ? DefaultText : (string)o;
            }
            set {
                ViewState["Text"] = value;
            }
        }
    }
}
