// MIT License.

using System.ComponentModel;

namespace System.Web.UI.WebControls.WebParts
{
    public interface IWebPartRow {
        PropertyDescriptorCollection Schema { get; }
        void GetRowData(RowCallback callback);
    }
}
