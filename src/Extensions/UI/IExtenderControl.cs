// MIT License.

using System.Diagnostics.CodeAnalysis;

namespace System.Web.UI
{
    public interface IExtenderControl {
        IEnumerable<ScriptDescriptor> GetScriptDescriptors(Control targetControl);

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",
            Justification = "Implementation will likely return a new collection, which is too slow for a property")]
        IEnumerable<ScriptReference> GetScriptReferences();
    }
}
