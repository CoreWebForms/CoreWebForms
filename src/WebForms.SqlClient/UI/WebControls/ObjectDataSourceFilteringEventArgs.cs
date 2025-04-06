//MIT license.

using System.Collections.Specialized;
using System.ComponentModel;

namespace System.Web.UI.WebControls;
/// <devdoc>
/// Event arguments for the ObjectDataSource Filter event.
/// </devdoc>
public class ObjectDataSourceFilteringEventArgs : CancelEventArgs {

    private readonly IOrderedDictionary _parameterValues;

    public ObjectDataSourceFilteringEventArgs(IOrderedDictionary parameterValues) {
        _parameterValues = parameterValues;
    }

    public IOrderedDictionary ParameterValues {
        get {
            return _parameterValues;
        }
    }
}

