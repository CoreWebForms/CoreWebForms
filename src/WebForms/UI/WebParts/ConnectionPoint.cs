// MIT License.

using System.Reflection;

namespace System.Web.UI.WebControls.WebParts
{
    /// <devdoc>
    /// A ConnectionPoint defines a possible connection. A WebPart uses this
    /// to define the connections it can provide or consume.
    /// </devdoc>
    public abstract class ConnectionPoint {
        private readonly MethodInfo _callbackMethod;
        private readonly Type _controlType;
        private readonly Type _interfaceType;
        private readonly string _displayName;
        private readonly string _id;
        private readonly bool _allowsMultipleConnections;

        // We do not want the public field to be "const", since that means we can never change its value.
        // We want the internal const field so we can use it in attributes.
        public static readonly string DefaultID = DefaultIDInternal;
        internal const string DefaultIDInternal = "default";

        // 

        internal ConnectionPoint(MethodInfo callbackMethod, Type interfaceType, Type controlType, string displayName, string id, bool allowsMultipleConnections) {
            if (callbackMethod == null) {
                throw new ArgumentNullException(nameof(callbackMethod));
            }

            if (interfaceType == null) {
                throw new ArgumentNullException(nameof(interfaceType));
            }

            if (controlType == null) {
                throw new ArgumentNullException(nameof(controlType));
            }

            if (!controlType.IsSubclassOf(typeof(Control))) {
                throw new ArgumentException(SR.GetString(SR.ConnectionPoint_InvalidControlType), nameof(controlType));
            }

            if (String.IsNullOrEmpty(displayName)) {
                throw new ArgumentNullException(nameof(displayName));
            }

            _callbackMethod = callbackMethod;
            _interfaceType = interfaceType;
            _controlType = controlType;
            _displayName = displayName;
            _id = id;
            _allowsMultipleConnections = allowsMultipleConnections;
        }

        public bool AllowsMultipleConnections {
            get {
                return _allowsMultipleConnections;
            }
        }

        internal MethodInfo CallbackMethod {
            get {
                return _callbackMethod;
            }
        }

        public Type ControlType {
            get {
                return _controlType;
            }
        }

        public Type InterfaceType {
            get {
                return _interfaceType;
            }
        }

        public string ID {
            get {
                return (!String.IsNullOrEmpty(_id)) ? _id : DefaultID;
            }
        }

        public string DisplayName {
            get {
                return _displayName;
            }
        }

        /// <devdoc>
        /// Base implementation returns true, can be overridden by subclasses to return
        /// true or false conditionally based on the state of the Control.
        /// </devdoc>
        public virtual bool GetEnabled(Control control) {
            return true;
        }
    }
}
