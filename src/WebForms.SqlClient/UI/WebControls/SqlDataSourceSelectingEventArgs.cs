// MIT License.

using System.Data.Common;

namespace System.Web.UI.WebControls;
public class SqlDataSourceSelectingEventArgs : SqlDataSourceCommandEventArgs
{

    private readonly DataSourceSelectArguments _arguments;
    /*private bool _executingSelectCount;*/

    public SqlDataSourceSelectingEventArgs(DbCommand command, DataSourceSelectArguments arguments /*, bool executingSelectCount*/) : base(command)
    {
        _arguments = arguments;
        //_executingSelectCount = executingSelectCount;
    }

    public DataSourceSelectArguments Arguments
    {
        get
        {
            return _arguments;
        }
    }

    /*public bool ExecutingSelectCount {
        get {
            return _executingSelectCount;
        }
    }*/
}

