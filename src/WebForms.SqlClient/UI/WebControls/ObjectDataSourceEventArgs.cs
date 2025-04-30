//MIT license.

namespace System.Web.UI.WebControls;
/// <devdoc>
/// Represents data that is passed into an ObjectDataSourceObjectEventHandler delegate.
/// </devdoc>
public class ObjectDataSourceEventArgs : EventArgs {

    private object _objectInstance;

    /// <devdoc>
    /// Creates a new instance of ObjectDataSourceEventArgs.
    /// </devdoc>
    public ObjectDataSourceEventArgs(object objectInstance) : base() {
        _objectInstance = objectInstance;
    }

    /// <devdoc>
    /// The instance of the object created by the ObjectDataSource. Set this
    /// property if you need to create the object using a non-default
    /// constructor.
    /// </devdoc>
    public object ObjectInstance {
        get {
            return _objectInstance;
        }
        set {
            _objectInstance = value;
        }
    }
}

