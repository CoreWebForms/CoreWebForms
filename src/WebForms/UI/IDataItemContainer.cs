// MIT License.

namespace System.Web.UI;
/// <devdoc>
/// </devdoc>
public interface IDataItemContainer : INamingContainer
{

    object DataItem
    {
        get;
    }

    int DataItemIndex
    {
        get;
    }

    int DisplayIndex
    {
        get;
    }
}

