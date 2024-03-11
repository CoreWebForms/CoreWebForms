// MIT License.

using System.Diagnostics;

namespace System.Web.UI
{
    public sealed class RegisteredArrayDeclaration
    {
        private readonly Control _control;
        private readonly string _name;
        private readonly string _value;

        internal RegisteredArrayDeclaration(Control control, string arrayName, string arrayValue)
        {
            Debug.Assert(arrayName != null);
            // null value allowed by asp.net
            _control = control;
            _name = arrayName;
            _value = arrayValue;
        }

        public string Name
        {
            get
            {
                return _name;
            }
        }

        public string Value
        {
            get
            {
                // may be null
                return _value;
            }
        }

        public Control Control
        {
            get
            {
                return _control;
            }
        }
    }
}
