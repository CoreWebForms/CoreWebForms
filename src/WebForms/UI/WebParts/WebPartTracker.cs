// MIT License.

using System.Diagnostics;

namespace System.Web.UI.WebControls.WebParts
{
    public sealed class WebPartTracker : IDisposable {
        private bool _disposed;
        private readonly WebPart _webPart;
        private readonly ProviderConnectionPoint _providerConnectionPoint;

        public WebPartTracker(WebPart webPart, ProviderConnectionPoint providerConnectionPoint) {
            if (webPart == null) {
                throw new ArgumentNullException(nameof(webPart));
            }

            if (providerConnectionPoint == null) {
                throw new ArgumentNullException(nameof(providerConnectionPoint));
            }

            if (providerConnectionPoint.ControlType != webPart.GetType()) {
                throw new ArgumentException(SR.GetString(SR.WebPartManager_InvalidConnectionPoint), nameof(providerConnectionPoint));
            }

            _webPart = webPart;
            _providerConnectionPoint = providerConnectionPoint;

            if (++Count > 1) {
                webPart.SetConnectErrorMessage(SR.GetString(SR.WebPartTracker_CircularConnection, _providerConnectionPoint.DisplayName));
            }
        }

        public bool IsCircularConnection {
            get {
                return (Count > 1);
            }
        }

        private int Count {
            get {
                int count;
                _webPart.TrackerCounter.TryGetValue(_providerConnectionPoint, out count);
                return count;
            }
            set {
                _webPart.TrackerCounter[_providerConnectionPoint] = value;
            }
        }

        void IDisposable.Dispose() {
            if (!_disposed) {
                Debug.Assert(Count >= 1);
                Count--;
                _disposed = true;
            }
        }
    }
}
