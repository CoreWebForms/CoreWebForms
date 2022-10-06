// MIT License.

namespace System.Web.UI.WebControls;

public class PageEventArgs : EventArgs
{
    private readonly int _startRowIndex;
    private readonly int _maximumRows;
    private readonly int _totalRowCount;

    public PageEventArgs(int startRowIndex, int maximumRows, int totalRowCount)
    {
        _startRowIndex = startRowIndex;
        _maximumRows = maximumRows;
        _totalRowCount = totalRowCount;
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

    public int TotalRowCount
    {
        get
        {
            return _totalRowCount;
        }
    }
}
