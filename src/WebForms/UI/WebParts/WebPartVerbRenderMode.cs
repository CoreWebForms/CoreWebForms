// MIT License.

namespace System.Web.UI.WebControls.WebParts
{
    /// <devdoc>
    /// Specifies how to render the WebPartVerbs.
    /// </devdoc>
    public enum WebPartVerbRenderMode {

        /// <devdoc>
        /// Render the WebPartVerbs in a popup menu in the WebPart TitleBar.
        /// </devdoc>
        Menu = 0,

        /// <devdoc>
        /// Render the WebPartVerbs as links or buttons directly in the WebPart TitleBar.
        /// This mode is keyboard accessible.
        /// </devdoc>
        TitleBar = 1,
    }
}
