// MIT License.

using System.ComponentModel;

namespace System.Web.UI.WebControls.WebParts
{
    public class WebPartCancelEventArgs : CancelEventArgs {
        private WebPart _webPart;

        public WebPartCancelEventArgs(WebPart webPart) {
            _webPart = webPart;
        }

        public WebPart WebPart {
            get {
                return _webPart;
            }
            set {
                _webPart = value;
            }
        }
    }
}
