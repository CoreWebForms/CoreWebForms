// MIT License.

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

    internal static string SerializeInternal(object o)
    {
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        return serializer.Serialize(o);
    }

    internal static object Deserialize(JavaScriptSerializer serializer, string input, Type type, int depthLimit)
    {
        if (input == null)
        {
            throw new ArgumentNullException(nameof(input));
        }
        if (input.Length > serializer.MaxJsonLength)
        {
            throw new ArgumentException(AtlasWeb.JSON_MaxJsonLengthExceeded, nameof(input));
        }

        var serializeOptions = new JsonSerializerOptions
        {
            MaxDepth = depthLimit
        };
        object result = JsonSerializer.Deserialize(input, type, serializeOptions);
        return ObjectConverter.ConvertObjectToType(result, type, serializer);
    }

    // INSTANCE fields/methods
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

    private Dictionary<Type, JavaScriptConverter> _converters;
    private Dictionary<Type, JavaScriptConverter> Converters
    {
        get
        {
            _converters ??= new Dictionary<Type, JavaScriptConverter>();
            return _converters;
        }
    }

    [SuppressMessage("Microsoft.Usage", "CA2301:EmbeddableTypesInContainersRule", MessageId = "Converters", Justification = "This is for managed types which need to have custom type converters for JSon serialization, I don't think there will be any com interop types for this scenario.")]
    public void RegisterConverters(IEnumerable<JavaScriptConverter> converters)
    {
        if (converters == null)
        {
            throw new ArgumentNullException(nameof(converters));
        }

        foreach (JavaScriptConverter converter in converters)
        {
            IEnumerable<Type> supportedTypes = converter.SupportedTypes;
            if (supportedTypes != null)
            {
                foreach (Type supportedType in supportedTypes)
                {
                    Converters[supportedType] = converter;
                }
            }
        }
    }

    [SuppressMessage("Microsoft.Usage", "CA2301:EmbeddableTypesInContainersRule", MessageId = "_converters", Justification = "This is for managed types which need to have custom type converters for JSon serialization, I don't think there will be any com interop types for this scenario.")]
    private JavaScriptConverter GetConverter(Type t)
    {
        if (_converters != null)
        {
            while (t != null)
            {
                if (_converters.ContainsKey(t))
                {
                    return _converters[t];
                }
                t = t.BaseType;
            }
        }
        return null;
    }

    internal bool ConverterExistsForType(Type t, out JavaScriptConverter converter)
    {
        converter = GetConverter(t);
        return converter != null;
    }

    public object DeserializeObject(string input)
    {
        return Deserialize(this, input, null /*type*/, RecursionLimit);
    }

    [
    SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
        Justification = "Generic parameter is preferable to forcing caller to downcast. " +
            "Has has been approved by API review board. " +
            "Dev10 701126: Overload added afterall, to allow runtime determination of the type.")
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
        Justification = "Generic parameter is preferable to forcing caller to downcast. " +
            "Has has been approved by API review board. " +
            "Dev10 701126: Overload added afterall, to allow runtime determination of the type."),
    SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "obj",
        Justification = "Cannot change parameter name as would break binary compatibility with legacy apps.")
    ]
    public T ConvertToType<T>(object obj)
    {
        return (T)ObjectConverter.ConvertObjectToType(obj, typeof(T), this);
    }

    [
    SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "obj",
        Justification = "Consistent with previously existing overload which cannot be changed.")
    ]
    public object ConvertToType(object obj, Type targetType)
    {
        return ObjectConverter.ConvertObjectToType(obj, targetType, this);
    }

    [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "obj",
        Justification = "Cannot change parameter name as would break binary compatibility with legacy apps.")]
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
        Justification = "Cannot change parameter name as would break binary compatibility with legacy apps.")]
    public void Serialize(object obj, StringBuilder output)
    {
        Serialize(obj, output, SerializationFormat.JSON);
    }

    internal void Serialize(object obj, StringBuilder output, SerializationFormat serializationFormat)
    {
        var serializeOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
        };
        serializeOptions.Converters.Add(new CustomJavaScriptConverter(this));

        var jsonString = JsonSerializer.Serialize(obj, serializeOptions);
        output.Append(jsonString);
        // DevDiv Bugs 96574: Max JSON length does not apply when serializing to Javascript for ScriptDescriptors
        if (serializationFormat == SerializationFormat.JSON && output.Length > MaxJsonLength)
        {
            throw new InvalidOperationException(AtlasWeb.JSON_MaxJsonLengthExceeded);
        }
    }

    internal sealed class CustomJavaScriptConverter(JavaScriptSerializer serializer) : JsonConverter<object>
    {
        private readonly JavaScriptSerializer _serializer = serializer;
        private JavaScriptConverter _converter;

        public override bool CanConvert(Type typeToConvert)
        {
            return _serializer.ConverterExistsForType(typeToConvert, out _converter);
        }

        public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException("De-serialization is not implemented");

        }

        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            if (_converter is null)
            {
                throw new Exception("Custom converter is not initialized");
            }

            var internalObject = _converter.Serialize(value, _serializer);
            JsonSerializer.Serialize(writer, internalObject, options);

        }
    }

    internal enum SerializationFormat
    {
        JSON,
        JavaScript
    }
}
