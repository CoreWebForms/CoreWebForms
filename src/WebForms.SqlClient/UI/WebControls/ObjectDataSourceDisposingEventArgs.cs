// MIT licensed

using System.ComponentModel;

namespace System.Web.UI.WebControls;

/// <devdoc>
/// Represents data that is passed into an ObjectDataSourceDisposingEventHandler delegate.
/// </devdoc>
public class ObjectDataSourceDisposingEventArgs : CancelEventArgs {

    private readonly object _objectInstance;

    /// <devdoc>
    /// Creates a new instance of ObjectDataSourceDisposingEventArgs.
    /// </devdoc>
    public ObjectDataSourceDisposingEventArgs(object objectInstance) : base() {
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
    }
}

