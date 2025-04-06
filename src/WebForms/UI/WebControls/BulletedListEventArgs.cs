//MIT License.

namespace System.Web.UI.WebControls;

/// <devdoc>
/// <para>The event args when a bulletedlist causes a postback.</para>
/// </devdoc>
public class BulletedListEventArgs : EventArgs {

    private readonly int _index;

    /// <devdoc>
    /// Constructor.
    /// </devdoc>
    /// <param name="index">The index of the element which caused the event.</param>
    public BulletedListEventArgs(int index) {
        _index = index;
    }

    /// <devdoc>
    /// The index of the element which caused the event.
    /// </devdoc>
    public int Index {
        get {
            return _index;
        }
    }
}

