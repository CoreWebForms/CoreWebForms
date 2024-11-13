// MIT License.

using System.Collections.Specialized;
using System.Text;
using Newtonsoft.Json;

namespace System.Web.Script.Serialization;

// TODO: We'll want to use System.Text.Json and change it to be strongly typed
internal sealed class JavaScriptSerializer
{
    public object MaxJsonLength { get; internal set; }

    internal static string SerializeInternal(OrderedDictionary orderedDictionary)
    {
        return JsonConvert.SerializeObject(orderedDictionary);
    }

    internal void Serialize(object value, StringBuilder builder, SerializationFormat javaScript)
    {
        var jsonString = JsonConvert.SerializeObject(value);
        builder.Append(jsonString);
    }

    internal void Serialize(string clientID, StringBuilder sb)
    {
        var jsonString = JsonConvert.SerializeObject(clientID);
        sb.Append(jsonString);
    }

    internal string Serialize(OrderedDictionary attrs)
    {
        return JsonConvert.SerializeObject(attrs);
    }

    internal string Serialize(string clientID)
    {
        return JsonConvert.SerializeObject(clientID);
    }

    public enum SerializationFormat
    {
        JavaScript
    }
}
