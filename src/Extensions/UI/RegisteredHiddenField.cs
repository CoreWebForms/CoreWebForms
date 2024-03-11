// MIT License.

using System.Diagnostics;

namespace System.Web.UI
{
    public sealed class RegisteredHiddenField
    {
        private readonly Control _control;
        private readonly string _name;
        private readonly string _initialValue;

        internal RegisteredHiddenField(Control control, string hiddenFieldName, string hiddenFieldInitialValue)
        {
            Debug.Assert(control != null);
            Debug.Assert(hiddenFieldName != null);
            _control = control;
            _name = hiddenFieldName;
            _initialValue = hiddenFieldInitialValue;
        }

        public Control Control
        {
            get
            {
                return _control;
            }
        }

        public string InitialValue
        {
            get
            {
                // may be null
                return _initialValue;
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
        }
    }
}
