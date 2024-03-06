// MIT License.

using System.ComponentModel;

namespace System.Web.UI.WebControls.WebParts
{
    public class WebPartDisplayModeCancelEventArgs : CancelEventArgs {
        private WebPartDisplayMode _newDisplayMode;

        public WebPartDisplayModeCancelEventArgs(WebPartDisplayMode newDisplayMode) {
            _newDisplayMode = newDisplayMode;
        }

        public WebPartDisplayMode NewDisplayMode {
            get {
                return _newDisplayMode;
            }
            set {
                _newDisplayMode = value;
            }
        }
    }
}
