// MIT License.

using System.Data.Common;

namespace System.Web.UI.WebControls;
public class SqlDataSourceStatusEventArgs : EventArgs
{

    private readonly DbCommand _command;
    private readonly Exception _exception;
    private bool _exceptionHandled;
    private readonly int _affectedRows;

    public SqlDataSourceStatusEventArgs(DbCommand command, int affectedRows, Exception exception) : base()
    {
        _command = command;
        _affectedRows = affectedRows;
        _exception = exception;
    }

    public int AffectedRows
    {
        get
        {
            return _affectedRows;
        }
    }

    public DbCommand Command
    {
        get
        {
            return _command;
        }
    }

    /// <devdoc>
    /// If an exception was thrown by the command, this property will contain the exception.
    /// If there was no exception, the value will be null.
    /// </devdoc>
    public Exception Exception
    {
        get
        {
            return _exception;
        }
    }

    /// <devdoc>
    /// If you wish to handle the exception using your own logic, set this value to true for it to be ignored by the control.
    /// If an exception was thrown and this value remains false, the exception will be re-thrown by the control.
    /// </devdoc>
    public bool ExceptionHandled
    {
        get
        {
            return _exceptionHandled;
        }
        set
        {
            _exceptionHandled = value;
        }
    }
}

