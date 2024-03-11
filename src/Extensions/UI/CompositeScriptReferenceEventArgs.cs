// MIT License.

namespace System.Web.UI
{
    public class CompositeScriptReferenceEventArgs : EventArgs
    {
        private readonly CompositeScriptReference _compositeScript;

        public CompositeScriptReferenceEventArgs(CompositeScriptReference compositeScript)
        {
            if (compositeScript == null)
            {
                throw new ArgumentNullException(nameof(compositeScript));
            }
            _compositeScript = compositeScript;
        }

        public CompositeScriptReference CompositeScript
        {
            get
            {
                return _compositeScript;
            }
        }
    }
}
