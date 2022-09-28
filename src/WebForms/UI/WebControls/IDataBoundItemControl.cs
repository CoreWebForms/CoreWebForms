// MIT License.

namespace System.Web.UI.WebControls;
public interface IDataBoundItemControl : IDataBoundControl
{
    DataKey DataKey
    {
        get;
    }

    DataBoundControlMode Mode
    {
        get;
    }
}
