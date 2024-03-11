// MIT License.

using System.Reflection;

namespace System.Web.UI.WebControls.WebParts
{
    /// <devdoc>
    /// Represents a property that has been marked as personalizable
    /// </devdoc>
    internal sealed class PersonalizablePropertyEntry {

        private readonly PropertyInfo _propertyInfo;
        private readonly PersonalizationScope _scope;
        private readonly bool _isSensitive;

        public PersonalizablePropertyEntry(PropertyInfo pi, PersonalizableAttribute attr) {
            _propertyInfo = pi;
            _scope = attr.Scope;
            _isSensitive = attr.IsSensitive;
        }

        public bool IsSensitive {
            get {
                return _isSensitive;
            }
        }

        public PersonalizationScope Scope {
            get {
                return _scope;
            }
        }

        public PropertyInfo PropertyInfo {
            get {
                return _propertyInfo;
            }
        }
    }
}
