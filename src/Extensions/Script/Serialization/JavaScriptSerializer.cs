// MIT License.

using System.Collections.Specialized;
using System.Text;

namespace System.Web.Script.Serialization;

// TODO: We'll want to use System.Text.Json and change it to be strongly typed
internal sealed class JavaScriptSerializer
{
    public object MaxJsonLength { get; internal set; }

    internal static string SerializeInternal(OrderedDictionary orderedDictionary)
    {
        throw new NotImplementedException();
    }

    internal void Serialize(object value, StringBuilder builder, SerializationFormat javaScript)
    {
        throw new NotImplementedException();
    }

    internal void Serialize(string clientID, StringBuilder sb)
    {
        throw new NotImplementedException();
    }

    internal string Serialize(OrderedDictionary attrs)
    {
        throw new NotImplementedException();
    }

    internal string Serialize(string clientID)
    {
        throw new NotImplementedException();
    }

    public enum SerializationFormat
    {
        JavaScript
    }
}
