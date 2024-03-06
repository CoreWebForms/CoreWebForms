// MIT License.

namespace System.Web.UI.WebControls.WebParts
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ConnectionConsumerAttribute : Attribute {

        private string _displayName;
        private readonly string _id;
        private readonly Type _connectionPointType;
        private bool _allowsMultipleConnections;

        public ConnectionConsumerAttribute(string displayName) {
            if (String.IsNullOrEmpty(displayName)) {
                throw new ArgumentNullException(nameof(displayName));
            }

            _displayName = displayName;
            _allowsMultipleConnections = false;
        }

        public ConnectionConsumerAttribute(string displayName, string id) : this(displayName) {
            if (String.IsNullOrEmpty(id)) {
                throw new ArgumentNullException(nameof(id));
            }

            _id = id;
        }

        public ConnectionConsumerAttribute(string displayName, Type connectionPointType) : this(displayName) {
            if (connectionPointType == null) {
                throw new ArgumentNullException(nameof(connectionPointType));
            }

            _connectionPointType = connectionPointType;
        }

        public ConnectionConsumerAttribute(string displayName, string id, Type connectionPointType) : this(displayName, connectionPointType) {
            if (String.IsNullOrEmpty(id)) {
                throw new ArgumentNullException(nameof(id));
            }

            _id = id;
        }

        public bool AllowsMultipleConnections {
            get {
                return _allowsMultipleConnections;
            }
            set {
                _allowsMultipleConnections = value;
            }
        }

        public string ID {
            get {
                return (_id != null) ? _id : String.Empty;
            }
        }

        public virtual string DisplayName {
            get {
                return DisplayNameValue;
            }
        }

        protected string DisplayNameValue {
            get {
                return _displayName;
            }
            set {
                _displayName = value;
            }
        }

        public Type ConnectionPointType {
            get {
                if (WebPartUtil.IsConnectionPointTypeValid(_connectionPointType, /*isConsumer*/ true)) {
                    return _connectionPointType;
                }
                else {
                    throw new InvalidOperationException(SR.GetString(SR.ConnectionConsumerAttribute_InvalidConnectionPointType, _connectionPointType.Name));
                }
            }
        }
    }
}

