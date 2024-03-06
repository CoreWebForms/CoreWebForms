// MIT License.

namespace System.Web.UI.WebControls.WebParts
{
    public class WebPartAuthorizationEventArgs : EventArgs {
        private readonly Type _type;
        private readonly string _path;
        private readonly string _authorizationFilter;
        private readonly bool _isShared;
        private bool _isAuthorized;

        public WebPartAuthorizationEventArgs(Type type, string path, string authorizationFilter, bool isShared) {
            _type = type;
            _path = path;
            _authorizationFilter = authorizationFilter;
            _isShared = isShared;
            _isAuthorized = true;
        }

        public string AuthorizationFilter {
            get {
                return _authorizationFilter;
            }
        }

        public bool IsAuthorized {
            get {
                return _isAuthorized;
            }
            set {
                _isAuthorized = value;
            }
        }

        public bool IsShared {
            get {
                return _isShared;
            }
        }

        public string Path {
            get {
                return _path;
            }
        }

        public Type Type {
            get {
                return _type;
            }
        }
    }
}
