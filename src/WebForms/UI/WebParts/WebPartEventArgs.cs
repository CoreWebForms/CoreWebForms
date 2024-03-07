// MIT License.

namespace System.Web.UI.WebControls.WebParts
{
    public class WebPartEventArgs : EventArgs {
        private readonly WebPart _webPart;

        public WebPartEventArgs(WebPart webPart) {
            _webPart = webPart;
        }

        public WebPart WebPart {
            get {
                return _webPart;
            }
        }
    }
}
