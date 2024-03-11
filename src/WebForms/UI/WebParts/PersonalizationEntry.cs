// MIT License.

namespace System.Web.UI.WebControls.WebParts
{
    public sealed class PersonalizationEntry {
        private PersonalizationScope _scope;
        private object _value;
        private bool _isSensitive;

        public PersonalizationEntry(object value, PersonalizationScope scope) : this(value, scope, false) {
        }

        public PersonalizationEntry(object value, PersonalizationScope scope, bool isSensitive) {
            PersonalizationProviderHelper.CheckPersonalizationScope(scope);

            _value = value;
            _scope = scope;
            _isSensitive = isSensitive;
        }

        public PersonalizationScope Scope {
            get {
                return _scope;
            }
            set {
                if (value < PersonalizationScope.User || value > PersonalizationScope.Shared) {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                _scope = value;
            }
        }

        public object Value {
            get {
                return _value;
            }
            set {
                _value = value;
            }
        }

        public bool IsSensitive {
            get {
                return _isSensitive;
            }
            set {
                _isSensitive = value;
            }
        }
    }
}

