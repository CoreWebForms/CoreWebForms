// MIT License.

using System.Web.Util;

namespace System.Web.UI.WebControls.WebParts
{
    /// <devdoc>
    /// </devdoc>
    public sealed class WebPartUserCapability {

        private readonly string _name;

        /// <devdoc>
        /// </devdoc>
        public WebPartUserCapability(string name) {
            if (String.IsNullOrEmpty(name)) {
                throw ExceptionUtil.ParameterNullOrEmpty("name");
            }

            _name = name;
        }

        /// <devdoc>
        /// </devdoc>
        public string Name {
            get {
                return _name;
            }
        }

        /// <devdoc>
        /// </devdoc>
        public override bool Equals(object o) {
            if (o == this) {
                return true;
            }

            WebPartUserCapability other = o as WebPartUserCapability;
            return (other != null) && (other.Name == Name);
        }

        /// <devdoc>
        /// </devdoc>
        public override int GetHashCode() {
            return _name.GetHashCode();
        }
    }
}
