// MIT License.

using System.Collections.Specialized;

namespace System.Web.UI
{
    public class HistoryEventArgs : EventArgs
    {
        private readonly NameValueCollection _state;

        public HistoryEventArgs(NameValueCollection state)
        {
            _state = state;
        }

        public NameValueCollection State
        {
            get { return _state; }
        }
    }
}
