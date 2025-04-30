//MIT license.

using System.Collections.Specialized;

namespace System.Web.UI.WebControls;
/// <devdoc>
/// Represents data that is passed into an ObjectDataSourceSelectingEventHandler delegate.
/// </devdoc>
public class ObjectDataSourceSelectingEventArgs : ObjectDataSourceMethodEventArgs {

    private readonly DataSourceSelectArguments _arguments;
    private readonly bool _executingSelectCount;

    /// <devdoc>
    /// Creates a new instance of ObjectDataSourceSelectingEventArgs.
    /// </devdoc>
    public ObjectDataSourceSelectingEventArgs(IOrderedDictionary inputParameters, DataSourceSelectArguments arguments, bool executingSelectCount) : base(inputParameters) {
        _arguments = arguments;
        _executingSelectCount = executingSelectCount;
    }

    public DataSourceSelectArguments Arguments {
        get {
            return _arguments;
        }
    }

    public bool ExecutingSelectCount {
        get {
            return _executingSelectCount;
        }
    }
}

