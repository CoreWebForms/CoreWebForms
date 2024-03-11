// MIT License.

using System.Diagnostics;

namespace System.Web.UI
{
    public sealed class RegisteredDisposeScript
    {
        private readonly Control _control;
        private readonly UpdatePanel _parentUpdatePanel;
        private readonly string _script;

        internal RegisteredDisposeScript(Control control, string disposeScript, UpdatePanel parentUpdatePanel)
        {
            Debug.Assert(control != null);
            Debug.Assert(disposeScript != null);
            Debug.Assert(parentUpdatePanel != null);
            _control = control;
            _script = disposeScript;
            _parentUpdatePanel = parentUpdatePanel;
        }

        public Control Control
        {
            get
            {
                return _control;
            }
        }

        public string Script
        {
            get
            {
                return _script;
            }
        }

        internal UpdatePanel ParentUpdatePanel
        {
            get
            {
                return _parentUpdatePanel;
            }
        }
    }
}
