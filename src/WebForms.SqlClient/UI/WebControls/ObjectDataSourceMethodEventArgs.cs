//MIT license.

using System.Collections.Specialized;
using System.ComponentModel;

namespace System.Web.UI.WebControls;
/// <devdoc>
/// Represents data that is passed into an ObjectDataSourceMethodEventHandler delegate.
/// </devdoc>
public class ObjectDataSourceMethodEventArgs : CancelEventArgs {

    private readonly IOrderedDictionary _inputParameters;

    /// <devdoc>
    /// Creates a new instance of ObjectDataSourceMethodEventArgs.
    /// </devdoc>
    public ObjectDataSourceMethodEventArgs(IOrderedDictionary inputParameters) {
        _inputParameters = inputParameters;
    }

    /// <devdoc>
    /// The input parameters that will be passed to the method that will be invoked.
    /// Change these parameters if the names and/or types need to be modified
    /// for the invocation to succeed.
    /// </devdoc>
    public IOrderedDictionary InputParameters {
        get {
            return _inputParameters;
        }
    }
}

