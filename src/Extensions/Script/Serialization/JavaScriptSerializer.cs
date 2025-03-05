// MIT License.

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web.Resources;

namespace System.Web.Script.Serialization;

public class JavaScriptSerializer
{
    internal const string ServerTypeFieldName = "__type";
    internal const int DefaultRecursionLimit = 100;
    internal const int DefaultMaxJsonLength = 2097152;
    private static readonly JavaScriptSerializer _defaultJavaScriptSerializer = new();

    private readonly JsonSerializerOptions _options = new()
    {
        MaxDepth = DefaultRecursionLimit,
    };

    internal static string SerializeInternal(object o)
    {
        return _defaultJavaScriptSerializer.Serialize(o);
    }

    internal static object Deserialize(JavaScriptSerializer serializer, string input, Type type, int depthLimit)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (input.Length > serializer.MaxJsonLength)
        {
            throw new ArgumentException(AtlasWeb.JSON_MaxJsonLengthExceeded, nameof(input));
        }

        return JsonSerializer.Deserialize(input, type, serializer._options);
    }

    private readonly JavaScriptTypeResolver _typeResolver;
    private int _recursionLimit;
    private int _maxJsonLength;

    public JavaScriptSerializer() : this(null) { }

    public JavaScriptSerializer(JavaScriptTypeResolver resolver)
    {
        _typeResolver = resolver;
        RecursionLimit = DefaultRecursionLimit;
        MaxJsonLength = DefaultMaxJsonLength;
    }

    public int MaxJsonLength
    {
        get
        {
            return _maxJsonLength;
        }
        set
        {
            if (value < 1)
            {
                throw new ArgumentOutOfRangeException(AtlasWeb.JSON_InvalidMaxJsonLength);
            }
            _maxJsonLength = value;
        }
    }

    public int RecursionLimit
    {
        get
        {
            return _recursionLimit;
        }
        set
        {
            if (value < 1)
            {
                throw new ArgumentOutOfRangeException(AtlasWeb.JSON_InvalidRecursionLimit);
            }
            _recursionLimit = value;
        }
    }

    internal JavaScriptTypeResolver TypeResolver
    {
        get
        {
            return _typeResolver;
        }
    }

    [SuppressMessage("Microsoft.Usage", "CA2301:EmbeddableTypesInContainersRule",
        MessageId = "Converters", Justification = Constant.CA1859)]
    public void RegisterConverters(IEnumerable<JavaScriptConverter> converters)
    {
        ArgumentNullException.ThrowIfNull(converters);

        foreach (var converter in converters)
        {
            _options.Converters.Add(new JavaScriptSerializerJsonConverterFactory(converter, this));
        }
    }

    [
    SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
        Justification = Constant.CA1004)
    ]
    public T Deserialize<T>(string input)
    {
        return (T)Deserialize(this, input, typeof(T), RecursionLimit);
    }

    public object Deserialize(string input, Type targetType)
    {
        return Deserialize(this, input, targetType, RecursionLimit);
    }

    [
    SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
        Justification = Constant.CA1004),
    SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "obj",
        Justification = Constant.CA1720)
    ]
    public T ConvertToType<T>(object obj)
    {
        return (T)ObjectConverter.ConvertObjectToType(obj, typeof(T), this);
    }

    [
    SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "obj",
        Justification = Constant.CA1720)
    ]
    public object ConvertToType(object obj, Type targetType)
    {
        return ObjectConverter.ConvertObjectToType(obj, targetType, this);
    }

    [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "obj",
        Justification = Constant.CA1720)]
    public string Serialize(object obj)
    {
        return Serialize(obj, SerializationFormat.JSON);
    }

    internal string Serialize(object obj, SerializationFormat serializationFormat)
    {
        StringBuilder sb = new StringBuilder();
        Serialize(obj, sb, serializationFormat);
        return sb.ToString();
    }

    [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "obj",
        Justification = Constant.CA1720)]
    public void Serialize(object obj, StringBuilder output)
    {
        Serialize(obj, output, SerializationFormat.JSON);
    }

    internal void Serialize(object obj, StringBuilder output, SerializationFormat serializationFormat)
    {
        var jsonString = JsonSerializer.Serialize(obj, _options);
        output.Append(jsonString);
        // DevDiv Bugs 96574: Max JSON length does not apply when serializing to Javascript for ScriptDescriptors
        if (serializationFormat == SerializationFormat.JSON && output.Length > MaxJsonLength)
        {
            throw new InvalidOperationException(AtlasWeb.JSON_MaxJsonLengthExceeded);
        }
    }

    internal bool ConverterExistsForType(Type type, out JavaScriptConverter converter)
    {
        foreach (var c in _options.Converters)
        {
            if (c is JavaScriptSerializerJsonConverterFactory factory)
            {
                if (factory.CanConvert(type))
                {
                    converter = factory.Converter;
                    return true;
                }
            }
        }

        converter = default;
        return false;
    }

    internal enum SerializationFormat
    {
        JSON,
        JavaScript
    }

    private sealed class JavaScriptSerializerJsonConverterFactory(JavaScriptConverter converter, JavaScriptSerializer serializer) : JsonConverterFactory
    {
        private readonly HashSet<Type> _types = [.. converter.SupportedTypes];
        private readonly JsonConverter _converter = new JavaScriptSerializerJsonConverter(converter, serializer);

        public JavaScriptConverter Converter => converter;

        public override bool CanConvert(Type typeToConvert) => _types.Contains(typeToConvert);

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) => _converter;
    }

    private sealed class JavaScriptSerializerJsonConverter(JavaScriptConverter converter, JavaScriptSerializer serializer) : JsonConverter<object>
    {
        public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var result = ReadDictionary(ref reader, typeToConvert, options);
            return converter.Deserialize(result, typeToConvert, serializer);
        }

        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, converter.Serialize(value, serializer), options);
        }

        private object ReadObject(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/converters-how-to#deserialize-inferred-types-to-object-properties
            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    if (reader.TryGetDateTime(out var date))
                    {
                        return date;
                    }
                    return reader.GetString();
                case JsonTokenType.False:
                    return false;
                case JsonTokenType.True:
                    return true;
                case JsonTokenType.Null:
                    return null;
                case JsonTokenType.Number:
                    if (reader.TryGetInt64(out var result))
                    {
                        return result;
                    }
                    return reader.GetDecimal();
                case JsonTokenType.StartObject:
                    return ReadDictionary(ref reader, typeToConvert, options);
                case JsonTokenType.StartArray:
                    var list = new ArrayList();
                    while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                    {
                        list.Add(ReadObject(ref reader, typeToConvert, options));
                    }
                    return list;
                default:
                    throw new JsonException($"'{reader.TokenType}' is not supported");
            }
        }

        private Dictionary<string, object> ReadDictionary(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dictionary = new Dictionary<string, object>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return dictionary;
                }
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException("JsonTokenType was not PropertyName");
                }
                var propertyName = reader.GetString();
                if (string.IsNullOrWhiteSpace(propertyName))
                {
                    throw new JsonException("Failed to get property name");
                }
                reader.Read();
                dictionary.Add(propertyName!, ReadObject(ref reader, typeToConvert, options));
            }
            return dictionary;
        }
    }
}
