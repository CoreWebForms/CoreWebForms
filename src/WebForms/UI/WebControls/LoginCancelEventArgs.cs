// MIT License.

namespace System.Web.UI.WebControls;

using System;

public class LoginCancelEventArgs : EventArgs
{

    private bool _cancel;

    public LoginCancelEventArgs() : this(false)
    {
    }

    public LoginCancelEventArgs(bool cancel)
    {
        _cancel = cancel;
    }

    public bool Cancel
    {
        get
        {
            return _cancel;
        }
        set
        {
            _cancel = value;
        }
    }
}
