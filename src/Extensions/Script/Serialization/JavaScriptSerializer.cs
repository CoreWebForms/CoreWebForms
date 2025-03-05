// MIT License.

#nullable enable

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

    private readonly JavaScriptTypeResolver? _typeResolver;
    private int _recursionLimit = DefaultRecursionLimit;
    private int _maxJsonLength = DefaultMaxJsonLength;

    private readonly JsonSerializerOptions _options = new()
    {
        MaxDepth = DefaultRecursionLimit,
    };

    public JavaScriptSerializer()
    {
    }

    public JavaScriptSerializer(JavaScriptTypeResolver? resolver)
    {
        _typeResolver = resolver;
    }

    internal static string SerializeInternal(object o) => _defaultJavaScriptSerializer.Serialize(o);

    internal static object? Deserialize(JavaScriptSerializer serializer, string input, Type type, int depthLimit)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (input.Length > serializer.MaxJsonLength)
        {
            throw new ArgumentException(AtlasWeb.JSON_MaxJsonLengthExceeded, nameof(input));
        }

        return JsonSerializer.Deserialize(input, type, serializer._options);
    }

    public int MaxJsonLength
    {
        get => _maxJsonLength;
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
        get => _recursionLimit;
        set
        {
            if (value < 1)
            {
                throw new ArgumentOutOfRangeException(AtlasWeb.JSON_InvalidRecursionLimit);
            }
            _recursionLimit = value;
        }
    }

    internal JavaScriptTypeResolver? TypeResolver => _typeResolver;

    public void RegisterConverters(IEnumerable<JavaScriptConverter> converters)
    {
        ArgumentNullException.ThrowIfNull(converters);

        foreach (var converter in converters)
        {
            _options.Converters.Add(new JavaScriptConverterWrapper(converter, this));
        }
    }

    public T? Deserialize<T>(string input) => (T?)Deserialize(this, input, typeof(T), RecursionLimit);

    public object? Deserialize(string input, Type targetType) => Deserialize(this, input, targetType, RecursionLimit);

    public T ConvertToType<T>(object obj) => (T)ObjectConverter.ConvertObjectToType(obj, typeof(T), this);

    public object ConvertToType(object obj, Type targetType) => ObjectConverter.ConvertObjectToType(obj, targetType, this);

    public string Serialize(object obj) => Serialize(obj, SerializationFormat.JSON);

    internal string Serialize(object obj, SerializationFormat serializationFormat)
    {
        StringBuilder sb = new StringBuilder();
        Serialize(obj, sb, serializationFormat);
        return sb.ToString();
    }

    public void Serialize(object obj, StringBuilder output) => Serialize(obj, output, SerializationFormat.JSON);

    internal void Serialize(object obj, StringBuilder output, SerializationFormat serializationFormat)
    {
        var jsonString = JsonSerializer.Serialize(obj, _options);
        output.Append(jsonString);

        if (serializationFormat == SerializationFormat.JSON && output.Length > MaxJsonLength)
        {
            throw new InvalidOperationException(AtlasWeb.JSON_MaxJsonLengthExceeded);
        }
    }

    internal bool ConverterExistsForType(Type type, [MaybeNullWhen(false)] out JavaScriptConverter converter)
    {
        foreach (var c in _options.Converters)
        {
            if (c is JavaScriptConverterWrapper factory)
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

    private sealed class JavaScriptConverterWrapper(JavaScriptConverter converter, JavaScriptSerializer serializer) : JsonConverter<object>
    {
        private readonly HashSet<Type> _types = [.. converter.SupportedTypes];

        public JavaScriptConverter Converter => converter;

        public override bool CanConvert(Type typeToConvert) => _types.Contains(typeToConvert);

        public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var result = ReadDictionary(ref reader, options);
            return converter.Deserialize(result, typeToConvert, serializer);
        }

        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
            => JsonSerializer.Serialize(writer, converter.Serialize(value, serializer), options);

        private static object? ReadObject(ref Utf8JsonReader reader, JsonSerializerOptions options)
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
                    return ReadDictionary(ref reader, options);
                case JsonTokenType.StartArray:
                    var list = new ArrayList();
                    while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                    {
                        list.Add(ReadObject(ref reader, options));
                    }
                    return list;
                default:
                    throw new JsonException($"'{reader.TokenType}' is not supported");
            }
        }

        private static Dictionary<string, object> ReadDictionary(ref Utf8JsonReader reader, JsonSerializerOptions options)
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

                if (ReadObject(ref reader, options) is { } value)
                {
                    dictionary.Add(propertyName, value);
                }
            }

            return dictionary;
        }
    }
}
