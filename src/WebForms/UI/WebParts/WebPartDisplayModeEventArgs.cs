// MIT License.

namespace System.Web.UI.WebControls.WebParts
{
    public class WebPartDisplayModeEventArgs : EventArgs {
        private WebPartDisplayMode _oldDisplayMode;

        public WebPartDisplayModeEventArgs(WebPartDisplayMode oldDisplayMode) {
            _oldDisplayMode = oldDisplayMode;
        }

        public WebPartDisplayMode OldDisplayMode {
            get {
                return _oldDisplayMode;
            }
            set {
                _oldDisplayMode = value;
            }
        }
    }
}
