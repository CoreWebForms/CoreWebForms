// MIT License.

namespace System.Web.UI.WebControls;
public interface IEditableTextControl : ITextControl
{
    /// <devdoc>
    ///     Raised when the text changes.
    /// </devdoc>
    event EventHandler TextChanged;
}
