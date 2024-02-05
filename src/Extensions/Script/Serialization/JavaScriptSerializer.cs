// MIT License.

using System.Text;

namespace System.Web.Script.Serialization;

internal class JavaScriptSerializer
{
    internal void Serialize(object value, StringBuilder builder, SerializationFormat javaScript)
    {
        throw new NotImplementedException();
    }

    public enum SerializationFormat
    {
        JavaScript
    }
}
