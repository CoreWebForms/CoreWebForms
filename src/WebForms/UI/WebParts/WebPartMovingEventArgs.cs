// MIT License.

namespace System.Web.UI.WebControls.WebParts
{
    public class WebPartMovingEventArgs : WebPartCancelEventArgs {
        private WebPartZoneBase _zone;
        private int _zoneIndex;

        public WebPartMovingEventArgs(WebPart webPart, WebPartZoneBase zone, int zoneIndex) : base(webPart) {
            _zone = zone;
            _zoneIndex = zoneIndex;
        }

        public WebPartZoneBase Zone {
            get {
                return _zone;
            }
            set {
                _zone = value;
            }
        }

        public int ZoneIndex {
            get {
                return _zoneIndex;
            }
            set {
                _zoneIndex = value;
            }
        }
    }
}

