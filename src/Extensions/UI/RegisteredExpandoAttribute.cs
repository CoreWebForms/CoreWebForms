// MIT License.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace System.Web.UI
{
    [
    SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "Consistent with RegisterExpandoAttribute API."),
    ]
    public sealed class RegisteredExpandoAttribute
    {
        private readonly Control _control;
        private readonly string _name;
        private readonly string _value;
        private readonly string _controlId;
        private readonly bool _encode;

        internal RegisteredExpandoAttribute(Control control,
            string controlId,
            string name,
            string value,
            bool encode)
        {

            Debug.Assert(control != null);
            Debug.Assert(!String.IsNullOrEmpty(controlId));
            Debug.Assert(!String.IsNullOrEmpty(name));
            // value can be null
            _control = control;
            _controlId = controlId;
            _name = name;
            _value = value;
            _encode = encode;
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

        public string ControlId
        {
            get
            {
                return _controlId;
            }
        }

        public bool Encode
        {
            get
            {
                return _encode;
            }
        }
    }
}
