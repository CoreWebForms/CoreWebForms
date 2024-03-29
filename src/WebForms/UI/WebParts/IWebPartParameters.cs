// MIT License.

using System.ComponentModel;

namespace System.Web.UI.WebControls.WebParts
{
    // Because of the SetConsumerSchema method, this interface only supports 1 consumer
    // at a time.  As a result, it is most useful when an IWebPartParameters consumer connects
    // to a provider via a transformer, like RowToParametersTransformer.  Alternatively, the
    // provider WebPart could specify AllowsMultipleConnections=false for the ConnectionPoint.
    public interface IWebPartParameters {
        PropertyDescriptorCollection Schema { get; }
        void GetParametersData(ParametersCallback callback);
        void SetConsumerSchema(PropertyDescriptorCollection schema);
    }
}
