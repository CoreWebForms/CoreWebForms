// MIT License.

namespace System.Web.UI.WebControls.WebParts
{
    public abstract class WebPartTransformer {

        /// <devdoc>
        /// Overridden by derived classes.  Should return a Control that implements
        /// ITransformerConfigurationControl
        /// </devdoc>
        public virtual Control CreateConfigurationControl() {
            return null;
        }

        protected internal virtual void LoadConfigurationState(object savedState) {
        }

        protected internal virtual object SaveConfigurationState() {
            return null;
        }

        public abstract object Transform(object providerData);
    }
}
