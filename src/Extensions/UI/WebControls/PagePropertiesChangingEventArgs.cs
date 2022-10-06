// MIT License.

namespace System.Web.UI.WebControls;

public class PagePropertiesChangingEventArgs : EventArgs
{
    private readonly int _startRowIndex;
    private readonly int _maximumRows;

    public PagePropertiesChangingEventArgs(int startRowIndex, int maximumRows)
    {
        _startRowIndex = startRowIndex;
        _maximumRows = maximumRows;
    }

    public int MaximumRows
    {
        get
        {
            return _maximumRows;
        }
    }

    public int StartRowIndex
    {
        get
        {
            return _startRowIndex;
        }
    }
}
