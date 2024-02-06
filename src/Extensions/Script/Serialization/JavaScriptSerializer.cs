// MIT License.

using System.Text;

namespace System.Web.Script.Serialization;

// TODO: We'll want to use System.Text.Json and change it to be strongly typed
internal sealed class JavaScriptSerializer
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
