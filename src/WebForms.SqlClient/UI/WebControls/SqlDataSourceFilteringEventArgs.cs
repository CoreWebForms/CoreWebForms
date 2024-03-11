// MIT License.

using System.Collections.Specialized;
using System.ComponentModel;

namespace System.Web.UI.WebControls;
/// <devdoc>
/// Event arguments for the SqlDataSource Filter event.
/// </devdoc>
public class SqlDataSourceFilteringEventArgs : CancelEventArgs
{

    private readonly IOrderedDictionary _parameterValues;

    public SqlDataSourceFilteringEventArgs(IOrderedDictionary parameterValues)
    {
        _parameterValues = parameterValues;
    }

    public IOrderedDictionary ParameterValues
    {
        get
        {
            return _parameterValues;
        }
    }
}

