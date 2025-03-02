// MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace System.Web.Script.Serialization;

internal sealed class CustomSerializerJsonConverter(JavaScriptSerializer serializer) : JsonConverter<object>
{
    public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new Exception("Shouldn't be called.");
    }

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        var converter = serializer.GetConverter(value.GetType());
        var internalObject = converter.Serialize(value, serializer);
        JsonSerializer.Serialize(writer, internalObject);
    }
}

//Factory to check if the type is supported by the JavaScriptSerializer
internal sealed class CustomSerializerFactory(JavaScriptSerializer serializer) : JsonConverterFactory
{
    private readonly JavaScriptSerializer _serializer = serializer;
    private JavaScriptConverter _converter;

    public override bool CanConvert(Type typeToConvert)
    {
        _converter = _serializer.GetConverter(typeToConvert);
        return (_converter != null);
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        return new CustomSerializerJsonConverter(_serializer);
    }
}
