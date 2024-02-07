// MIT License.

using System.Diagnostics.CodeAnalysis;

namespace System.Web.UI
{
    public abstract class ScriptDescriptor {
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",
            Justification = "Implementation will likely be too slow for a property")]
        protected internal abstract string GetScript();

        internal virtual void RegisterDisposeForDescriptor(ScriptManager scriptManager, Control owner) {
        }
    }
}
