// MIT License.

namespace System.Web.UI.WebControls.WebParts
{
    public class WebPartVerbsEventArgs : EventArgs {
        private WebPartVerbCollection _verbs;

        public WebPartVerbsEventArgs() : this(null) {
        }

        public WebPartVerbsEventArgs(WebPartVerbCollection verbs) {
            _verbs = verbs;
        }

        public WebPartVerbCollection Verbs {
            get {
                return (_verbs != null) ? _verbs : WebPartVerbCollection.Empty;
            }
            set {
                _verbs = value;
            }
        }
    }
}
