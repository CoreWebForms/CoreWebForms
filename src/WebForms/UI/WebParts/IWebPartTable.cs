// MIT License.

using System.ComponentModel;

namespace System.Web.UI.WebControls.WebParts
{
    public interface IWebPartTable {
        PropertyDescriptorCollection Schema { get; }
        void GetTableData(TableCallback callback);
    }
}
