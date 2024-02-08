// MIT License.

namespace System.Web.UI {
    using System;
    using System.Web;

    public class ScriptReferenceEventArgs : EventArgs {
        private readonly ScriptReference _script;

        public ScriptReferenceEventArgs(ScriptReference script) {
            if (script == null) {
                throw new ArgumentNullException(nameof(script));
            }
            _script = script;
        }

        public ScriptReference Script {
            get {
                return _script;
            }
        }
    }
}
