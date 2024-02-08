// MIT License.

namespace System.Web.UI
{
    using System;
    using System.Web;

    public class AsyncPostBackErrorEventArgs : EventArgs
    {
        private readonly Exception _exception;

        public AsyncPostBackErrorEventArgs(Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }
            _exception = exception;
        }

        public Exception Exception
        {
            get
            {
                return _exception;
            }
        }
    }
}
