// MIT License.

using System.Collections;

namespace System.Web.UI.WebControls.WebParts
{
    /// <devdoc>
    /// Allows an object to load up personalization state that would
    /// otherwise be discarded as a result of loading property values
    /// that have been removed in a newer version of the object's
    /// implementation.
    /// </devdoc>
    public interface IVersioningPersonalizable {

        /// <devdoc>
        /// The specified dictionary contains property values that
        /// could not automatically be loaded into the object.
        /// </devdoc>
        void Load(IDictionary unknownProperties);
    }
}
