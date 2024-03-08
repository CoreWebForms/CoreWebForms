// MIT License.

using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;

namespace System.Web.UI.WebControls;
public class SqlDataSourceCommandEventArgs : CancelEventArgs
{

    private DbCommand _command;

    public SqlDataSourceCommandEventArgs(DbCommand command) : base()
    {
        _command = command;
    }

    public DbCommand Command
    {
        get
        {
            return _command;
        }
    }
}

