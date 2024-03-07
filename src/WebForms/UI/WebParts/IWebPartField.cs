// MIT License.

using System.ComponentModel;

namespace System.Web.UI.WebControls.WebParts
{
    public interface IWebPartField {
        PropertyDescriptor Schema { get; }
        void GetFieldValue(FieldCallback callback);
    }
}
