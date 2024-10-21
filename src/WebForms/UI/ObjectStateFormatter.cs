// MIT License.

using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web.UI.WebControls;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.SystemWebAdapters;

#nullable disable

namespace System.Web.UI;
/// <devdoc>
/// ObjectStateFormatter is designed to efficiently serialize arbitrary object graphs
/// that represent the state of an object (decomposed into simpler types) into
/// a highly compact binary or ASCII representations.
/// The formatter contains native support for optimized serialization of a fixed
/// set of known types such as ints, shorts, booleans, strings, other primitive types
/// arrays, Pairs, Triplets, ArrayLists, Hashtables etc. In addition it utilizes
/// TypeConverters for semi-optimized serialization of custom types. Finally, it uses
/// binary serialization as a fallback mechanism. The formatter is also able to compress
/// IndexedStrings contained in the object graph.
/// </devdoc>
public sealed class ObjectStateFormatter : IStateFormatter, IStateFormatter2
{
    // Optimized type tokens
    private const byte Token_Int16 = 1;
    private const byte Token_Int32 = 2;
    private const byte Token_Byte = 3;
    private const byte Token_Char = 4;
    private const byte Token_String = 5;
    private const byte Token_DateTime = 6;
    private const byte Token_Double = 7;
    private const byte Token_Single = 8;
    private const byte Token_Color = 9;
    private const byte Token_KnownColor = 10;
    private const byte Token_IntEnum = 11;
    private const byte Token_EmptyColor = 12;
    private const byte Token_Pair = 15;
    private const byte Token_Triplet = 16;
    private const byte Token_Array = 20;
    private const byte Token_StringArray = 21;
    private const byte Token_ArrayList = 22;
    private const byte Token_Hashtable = 23;
    private const byte Token_HybridDictionary = 24;
    private const byte Token_Type = 25;
    private const byte Token_Unit = 27;
    private const byte Token_EmptyUnit = 28;
    private const byte Token_EventValidationStore = 29;

    // String-table optimized strings
    private const byte Token_IndexedStringAdd = 30;
    private const byte Token_IndexedString = 31;

    // Semi-optimized (TypeConverter-based)
    private const byte Token_StringFormatted = 40;

    // Semi-optimized (Types)
    private const byte Token_TypeRefAdd = 41;
    private const byte Token_TypeRefAddLocal = 42;
    private const byte Token_TypeRef = 43;

    // #if PORT_BINARYSERIALIZER
    // Un-optimized (Binary serialized) types
    private const byte Token_BinarySerialized = 50;
    // #endif

    // Optimized for sparse arrays
    private const byte Token_SparseArray = 60;

    // Constant values
    private const byte Token_Null = 100;
    private const byte Token_EmptyString = 101;
    private const byte Token_ZeroInt32 = 102;
    private const byte Token_True = 103;
    private const byte Token_False = 104;

    // Known types for which we generate short type references
    // rather than assembly qualified names
    //

    private static readonly Type[] KnownTypes =
        new Type[] {
            typeof(object),
            typeof(int),
            typeof(string),
            typeof(bool)
        };

    // Format and Version
    private const byte Marker_Format = 0xFF;
    private const byte Marker_Version_1 = 0x01;

    // The size of the string table. At most it can be Byte.MaxValue.
    //
    private const int StringTableSize = byte.MaxValue;

    // Used during serialization
    private IDictionary _typeTable;
    private IDictionary _stringTable;

    // Used during deserialization
    private IList _typeList;

    // Used during both serialization and deserialization
    private int _stringTableCount;
    private string[] _stringList;

    // If true, this class will throw an exception if it cannot deserialize a type or value.
    // If false, this class will use insert "null" if it cannot deserialize a type or value.
    // Default is true, WebParts Personalization sets this to false.
    private const bool _throwOnErrorDeserializing = false;

    /// <devdoc>
    /// Initializes a new instance of the ObjectStateFormatter.
    /// </devdoc>
    public ObjectStateFormatter()
    {
    }

    public ObjectStateFormatter(Page page, bool b)
    {
    }

    /// <devdoc>
    /// Adds a string reference during the deserialization process
    /// to support deserialization of IndexedStrings.
    /// The string is added to the string list on the fly, so it is available
    /// for future reference by index.
    /// </devdoc>
    private void AddDeserializationStringReference(string s)
    {
        Debug.Assert((s != null) && (s.Length != 0));

        if (_stringTableCount == StringTableSize)
        {
            // loop around to the start of the table
            _stringTableCount = 0;
        }

        _stringList[_stringTableCount] = s;
        _stringTableCount++;
    }

    /// <devdoc>
    /// Adds a type reference during the deserialization process,
    /// so that it can be referred to later by its index.
    /// </devdoc>
    private void AddDeserializationTypeReference(Type type) =>
        // Type may be null, if there is no longer a Type on the system with the saved type name.
        // This is unlikely to happen with a Type stored in ViewState, but more likely with a Type
        // stored in Personalization.
        _typeList.Add(type);

    /// <devdoc>
    /// Adds a string reference during the serialization process to support
    /// the serialization of IndexedStrings.
    /// The string is added to the string list, as well as to a string table
    /// for quick lookup.
    /// </devdoc>
    private void AddSerializationStringReference(string s)
    {
        Debug.Assert((s != null) && (s.Length != 0));

        if (_stringTableCount == StringTableSize)
        {
            // loop around to the start of the table
            _stringTableCount = 0;
        }

        var oldString = _stringList[_stringTableCount];
        if (oldString != null)
        {
            // it means we're looping around, and the existing table entry
            // needs to be removed, as a new one will replace it
            Debug.Assert(_stringTable.Contains(oldString));
            _stringTable.Remove(oldString);
        }

        _stringTable[s] = _stringTableCount;
        _stringList[_stringTableCount] = s;
        _stringTableCount++;
    }

    /// <devdoc>
    /// Adds a type reference during the serialization process, so it
    /// can be later referred to by its index.
    /// </devdoc>
    private void AddSerializationTypeReference(Type type)
    {
        Debug.Assert(type != null);

        var typeID = _typeTable.Count;
        _typeTable[type] = typeID;
    }

    internal object DeserializeWithAssert(Stream inputStream) => Deserialize(inputStream);

    /// <devdoc>
    /// Deserializes an object graph from its binary serialized form
    /// contained in the specified stream.
    /// </devdoc>
    public object Deserialize(Stream inputStream)
    {
        if (inputStream == null)
        {
            throw new ArgumentNullException(nameof(inputStream));
        }

        Exception deserializationException = null;

        InitializeDeserializer();

        var reader = new BinaryReader(inputStream);
        try
        {
            var formatMarker = reader.ReadByte();

            if (formatMarker == Marker_Format)
            {
                var versionMarker = reader.ReadByte();

                Debug.Assert(versionMarker == Marker_Version_1);
                if (versionMarker == Marker_Version_1)
                {
                    return DeserializeValue(reader);
                }
            }
        }
        catch (Exception e)
        {
            deserializationException = e;
        }

        // throw an exception if there was an exception during deserialization
        // or if deserialization was skipped because of invalid format or
        // version data in the stream

        throw new ArgumentException(SR.GetString(SR.InvalidSerializedData), deserializationException);
    }

    /// <devdoc>
    /// Deserializes an object graph from its textual serialized form
    /// contained in the specified string.
    /// </devdoc>
    public object Deserialize(string inputString) =>
        // If the developer called Deserialize() manually on an ObjectStateFormatter object that was configured
        // for cryptographic operations, he wouldn't have been able to specify a Purpose. We'll just provide
        // a default value for him.
        Deserialize(inputString, Purpose.User_ObjectStateFormatter_Serialize);

    private IDataProtector _protector;

    private IDataProtector Protector
    {
        get
        {
            if (_protector is null)
            {
                var provider = HttpContext.Current.AsAspNetCore().RequestServices.GetDataProtectionProvider();
                _protector = provider.CreateProtector("SystemWebForms");
            }

            return _protector;
        }
    }

    private object Deserialize(string inputString, Purpose purpose)
    {
        if (string.IsNullOrEmpty(inputString))
        {
            throw new ArgumentNullException(nameof(inputString));
        }

        var inputBytes = Convert.FromBase64String(inputString);
        var unprotected = Protector.Unprotect(inputBytes);

        using var objectStream = new MemoryStream(unprotected);

        return Deserialize(objectStream);
    }

    /// <devdoc>
    /// Deserializes an IndexedString. An IndexedString can either be the string itself (the
    /// first occurrence), or a reference to it by index into the string table.
    /// </devdoc>
    private IndexedString DeserializeIndexedString(BinaryReader reader, byte token)
    {
        Debug.Assert((token == Token_IndexedStringAdd) || (token == Token_IndexedString));

        if (token == Token_IndexedString)
        {
            // reference to string in the current string table
            var tableIndex = (int)reader.ReadByte();

            Debug.Assert(_stringList[tableIndex] != null);
            return new IndexedString(_stringList[tableIndex]);
        }
        else
        {
            // first occurrence of this indexed string. Read in the string, and add
            // a reference to it, so future references can be resolved.
            var s = reader.ReadString();

            AddDeserializationStringReference(s);
            return new IndexedString(s);
        }
    }

    /// <devdoc>
    /// Deserializes a Type. A Type can either be its name (the first occurrence),
    /// or a reference to it by index into the type table.  If we cannot load the type,
    /// we throw an exception if _throwOnErrorDeserializing is true, and we return null if
    /// _throwOnErrorDeserializing is false.
    /// </devdoc>
    private Type DeserializeType(BinaryReader reader)
    {
        var token = reader.ReadByte();
        Debug.Assert((token == Token_TypeRef) ||
                     (token == Token_TypeRefAdd) ||
                     (token == Token_TypeRefAddLocal));

        if (token == Token_TypeRef)
        {
            // reference by index into type table
            var typeID = reader.Read7BitEncodedInt();
            return (Type)_typeList[typeID];
        }
        else
        {
            // first occurrence of this type. Read in the type, resolve it, and
            // add it to the type table
            var typeName = reader.ReadString();

            Type resolvedType = null;
            try
            {
                if (token == Token_TypeRefAddLocal)
                {
                    resolvedType = typeof(ObjectStateFormatter).Assembly.GetType(typeName, true);
                }
                else
                {
                    resolvedType = Type.GetType(typeName, true);
                }
            }
            catch
            {
                if (_throwOnErrorDeserializing)
                {
                    throw;
                }
            }

            AddDeserializationTypeReference(resolvedType);
            return resolvedType;
        }
    }

    /// <devdoc>
    /// Deserializes a single value from the underlying stream.
    /// Essentially a token is read, followed by as much data needed to recreate
    /// the single value.
    /// </devdoc>
    private object DeserializeValue(BinaryReader reader)
    {
        var token = reader.ReadByte();

        // NOTE: Preserve the order here with the order of the logic in
        //       the SerializeValue method.

        switch (token)
        {
            case Token_Null:
                return null;
            case Token_EmptyString:
                return string.Empty;
            case Token_String:
                return reader.ReadString();
            case Token_ZeroInt32:
                return 0;
            case Token_Int32:
                return reader.Read7BitEncodedInt();
            case Token_Pair:
                return new Pair(DeserializeValue(reader),
                                DeserializeValue(reader));
            case Token_Triplet:
                return new Triplet(DeserializeValue(reader),
                                   DeserializeValue(reader),
                                   DeserializeValue(reader));
            case Token_IndexedString:
            case Token_IndexedStringAdd:
                return DeserializeIndexedString(reader, token);
            case Token_ArrayList:
                {
                    var count = reader.Read7BitEncodedInt();
                    var list = new ArrayList(count);
                    for (var i = 0; i < count; i++)
                    {
                        list.Add(DeserializeValue(reader));
                    }

                    return list;
                }
            case Token_True:
                return true;
            case Token_False:
                return false;
            case Token_Byte:
                return reader.ReadByte();
            case Token_Char:
                return reader.ReadChar();
            case Token_DateTime:
                return DateTime.FromBinary(reader.ReadInt64());
            case Token_Double:
                return reader.ReadDouble();
            case Token_Int16:
                return reader.ReadInt16();
            case Token_Single:
                return reader.ReadSingle();
            case Token_Hashtable:
            case Token_HybridDictionary:
                {
                    var count = reader.Read7BitEncodedInt();

                    IDictionary table;
                    if (token == Token_Hashtable)
                    {
                        table = new Hashtable(count);
                    }
                    else
                    {
                        table = new HybridDictionary(count);
                    }
                    for (var i = 0; i < count; i++)
                    {
                        table.Add(DeserializeValue(reader),
                                  DeserializeValue(reader));
                    }

                    return table;
                }
            case Token_Type:
                return DeserializeType(reader);
            case Token_StringArray:
                {
                    var count = reader.Read7BitEncodedInt();

                    var array = new string[count];
                    for (var i = 0; i < count; i++)
                    {
                        array[i] = reader.ReadString();
                    }

                    return array;
                }
            case Token_Array:
                {
                    var elementType = DeserializeType(reader);
                    var count = reader.Read7BitEncodedInt();

                    var list = Array.CreateInstance(elementType, count);
                    for (var i = 0; i < count; i++)
                    {
                        list.SetValue(DeserializeValue(reader), i);
                    }

                    return list;
                }
            case Token_IntEnum:
                {
                    var enumType = DeserializeType(reader);
                    var enumValue = reader.Read7BitEncodedInt();

                    return Enum.ToObject(enumType, enumValue);
                }
            case Token_Color:
                return Color.FromArgb(reader.ReadInt32());
            case Token_EmptyColor:
                return Color.Empty;
            case Token_KnownColor:
                return Color.FromKnownColor((KnownColor)reader.Read7BitEncodedInt());
            case Token_Unit:
                return new Unit(reader.ReadDouble(), (UnitType)reader.ReadInt32());
            case Token_EmptyUnit:
                return Unit.Empty;
            case Token_EventValidationStore:
                return EventValidationStore.DeserializeFrom(reader.BaseStream);
            case Token_SparseArray:
                {
                    var elementType = DeserializeType(reader);
                    var count = reader.Read7BitEncodedInt();
                    var itemCount = reader.Read7BitEncodedInt();

                    // Guard against bad data
                    if (itemCount > count)
                    {
                        throw new InvalidOperationException(SR.GetString(SR.InvalidSerializedData));
                    }

                    var list = Array.CreateInstance(elementType, count);
                    for (var i = 0; i < itemCount; ++i)
                    {
                        // Data is encoded as <index, Item>
                        var nextPos = reader.Read7BitEncodedInt();

                        // Guard against bad data (nextPos way too big, or nextPos not increasing)
                        if (nextPos >= count || nextPos < 0)
                        {
                            throw new InvalidOperationException(SR.GetString(SR.InvalidSerializedData));
                        }
                        list.SetValue(DeserializeValue(reader), nextPos);
                    }

                    return list;
                }
            case Token_StringFormatted:
                {
                    object result = null;

                    var valueType = DeserializeType(reader);
                    var formattedValue = reader.ReadString();

                    if (valueType != null)
                    {
                        var converter = TypeDescriptor.GetConverter(valueType);
                        // TypeDescriptor.GetConverter() will never return null.  The ref docs
                        // for this method are incorrect.
                        try
                        {
                            result = converter.ConvertFromInvariantString(formattedValue);
                        }
                        catch (Exception)
                        {
                            if (_throwOnErrorDeserializing)
                            {
                                throw;
                            }
                        }
                    }

                    return result;
                }
            case Token_BinarySerialized:
                {
                    var length = reader.Read7BitEncodedInt();

                    var buffer = new byte[length];
                    if (length != 0)
                    {
                        reader.Read(buffer, 0, length);
                    }

                    object result = null;

                    using (MemoryStream ms = new())
                    {
                        try
                        {
                            ms.Write(buffer, 0, length);
                            ms.Position = 0;

#pragma warning disable SYSLIB0011
                            IFormatter formatter = new BinaryFormatter();
#pragma warning restore SYSLIB0011

                            result = formatter.Deserialize(ms);
                        }
                        catch (Exception exception)
                        {
                            if (_throwOnErrorDeserializing)
                            {
                                throw;
                            }
                        }
                    }

                    return result;
                }
            default:
                throw new InvalidOperationException(SR.GetString(SR.InvalidSerializedData));
        }
    }

    /// <devdoc>
    /// Initializes this instance to perform deserialization.
    /// </devdoc>
    private void InitializeDeserializer()
    {
        _typeList = new ArrayList();

        for (var i = 0; i < KnownTypes.Length; i++)
        {
            AddDeserializationTypeReference(KnownTypes[i]);
        }

        _stringList = new string[byte.MaxValue];
        _stringTableCount = 0;
    }

    /// <devdoc>
    /// Initializes this instance to perform serialization.
    /// </devdoc>
    private void InitializeSerializer()
    {
        _typeTable = new HybridDictionary();

        for (var i = 0; i < KnownTypes.Length; i++)
        {
            AddSerializationTypeReference(KnownTypes[i]);
        }

        _stringList = new string[byte.MaxValue];
        _stringTable = new Hashtable(StringComparer.Ordinal);
        _stringTableCount = 0;
    }

    /// <devdoc>
    /// Serializes an object graph into a textual serialized form.
    /// </devdoc>
    public string Serialize(object stateGraph) =>
        // If the developer called Serialize() manually on an ObjectStateFormatter object that was configured
        // for cryptographic operations, he wouldn't have been able to specify a Purpose. We'll just provide
        // a default value for him.
        Serialize(stateGraph, Purpose.User_ObjectStateFormatter_Serialize);

    private string Serialize(object stateGraph, Purpose purpose)
    {
        using var ms = new MemoryStream();
        Serialize(ms, stateGraph);

        var buffer = Protector.Protect(ms.ToArray());

        return Convert.ToBase64String(buffer);
    }

    internal void SerializeWithAssert(Stream outputStream, object stateGraph) => Serialize(outputStream, stateGraph);

    /// <devdoc>
    /// Serializes an object graph into a binary serialized form within
    /// the specified stream.
    /// </devdoc>
    public void Serialize(Stream outputStream, object stateGraph)
    {
        if (outputStream == null)
        {
            throw new ArgumentNullException(nameof(outputStream));
        }

        InitializeSerializer();

        var writer = new BinaryWriter(outputStream);
        writer.Write(Marker_Format);
        writer.Write(Marker_Version_1);
        SerializeValue(writer, stateGraph);
    }

    /// <devdoc>
    /// Serializes an IndexedString. If this is the first occurrence, it is written
    /// out to the underlying stream, and is added to the string table for future
    /// reference. Otherwise, a reference by index is written out.
    /// </devdoc>
    private void SerializeIndexedString(BinaryWriter writer, string s)
    {
        var id = _stringTable[s];
        if (id != null)
        {
            writer.Write(Token_IndexedString);
            writer.Write((byte)(int)id);
            return;
        }

        AddSerializationStringReference(s);

        writer.Write(Token_IndexedStringAdd);
        writer.Write(s);
    }

    /// <devdoc>
    /// Serializes a Type. If this is the first occurrence, the type name is written
    /// out to the underlying stream, and the type is added to the string table for future
    /// reference. Otherwise, a reference by index is written out.
    /// </devdoc>
    private void SerializeType(BinaryWriter writer, Type type)
    {
        var id = _typeTable[type];
        if (id != null)
        {
            writer.Write(Token_TypeRef);
            writer.Write7BitEncodedInt((int)id);
            return;
        }

        AddSerializationTypeReference(type);

        if (type.Assembly == typeof(ObjectStateFormatter).Assembly)
        {
            writer.Write(Token_TypeRefAddLocal);
            writer.Write(type.FullName);
        }
        else
        {
            writer.Write(Token_TypeRefAdd);
            writer.Write(type.AssemblyQualifiedName);
        }
    }

    /// <devdoc>
    /// Serializes a single value using the specified writer.
    /// Handles exceptions to provide more information about the value being serialized.
    /// </devdoc>
    [Obsolete("Obsolete")]
    private void SerializeValue(BinaryWriter writer, object value)
    {
        try
        {

            var objectStack = new Stack();
            objectStack.Push(value);

            do
            {
                value = objectStack.Pop();

                if (value == null)
                {
                    writer.Write(Token_Null);
                    continue;
                }

                // NOTE: These are ordered roughly in the order of frequency.

                if (value is string)
                {
                    var s = (string)value;
                    if (s.Length == 0)
                    {
                        writer.Write(Token_EmptyString);
                    }
                    else
                    {
                        writer.Write(Token_String);
                        writer.Write(s);
                    }
                    continue;
                }

                if (value is int)
                {
                    var i = (int)value;
                    if (i == 0)
                    {
                        writer.Write(Token_ZeroInt32);
                    }
                    else
                    {
                        writer.Write(Token_Int32);
                        writer.Write7BitEncodedInt(i);
                    }
                    continue;
                }

                if (value is Pair)
                {
                    writer.Write(Token_Pair);

                    var p = (Pair)value;
                    objectStack.Push(p.Second);
                    objectStack.Push(p.First);
                    continue;
                }

                if (value is Triplet)
                {
                    writer.Write(Token_Triplet);

                    var t = (Triplet)value;
                    objectStack.Push(t.Third);
                    objectStack.Push(t.Second);
                    objectStack.Push(t.First);
                    continue;
                }

                if (value is IndexedString)
                {
                    Debug.Assert(((IndexedString)value).Value != null);
                    SerializeIndexedString(writer, ((IndexedString)value).Value);
                    continue;
                }

                if (value.GetType() == typeof(ArrayList))
                {
                    writer.Write(Token_ArrayList);

                    var list = (ArrayList)value;

                    writer.Write7BitEncodedInt(list.Count);
                    for (var i = list.Count - 1; i >= 0; i--)
                    {
                        objectStack.Push(list[i]);
                    }

                    continue;
                }

                if (value is bool)
                {
                    if (((bool)value))
                    {
                        writer.Write(Token_True);
                    }
                    else
                    {
                        writer.Write(Token_False);
                    }
                    continue;
                }
                if (value is byte)
                {
                    writer.Write(Token_Byte);
                    writer.Write((byte)value);
                    continue;
                }
                if (value is char)
                {
                    writer.Write(Token_Char);
                    writer.Write((char)value);
                    continue;
                }
                if (value is DateTime)
                {
                    writer.Write(Token_DateTime);
                    writer.Write(((DateTime)value).ToBinary());
                    continue;
                }
                if (value is double)
                {
                    writer.Write(Token_Double);
                    writer.Write((double)value);
                    continue;
                }
                if (value is short)
                {
                    writer.Write(Token_Int16);
                    writer.Write((short)value);
                    continue;
                }
                if (value is float)
                {
                    writer.Write(Token_Single);
                    writer.Write((float)value);
                    continue;
                }

                if (value is IDictionary)
                {
                    var canSerializeDictionary = false;

                    if (value.GetType() == typeof(Hashtable))
                    {
                        writer.Write(Token_Hashtable);
                        canSerializeDictionary = true;
                    }
                    else if (value.GetType() == typeof(HybridDictionary))
                    {
                        writer.Write(Token_HybridDictionary);
                        canSerializeDictionary = true;
                    }

                    if (canSerializeDictionary)
                    {
                        var table = (IDictionary)value;

                        writer.Write7BitEncodedInt(table.Count);
                        if (table.Count != 0)
                        {
                            foreach (DictionaryEntry entry in table)
                            {
                                objectStack.Push(entry.Value);
                                objectStack.Push(entry.Key);
                            }
                        }

                        continue;
                    }
                }

                if (value is EventValidationStore)
                {
                    writer.Write(Token_EventValidationStore);
                    ((EventValidationStore)value).SerializeTo(writer.BaseStream);
                    continue;
                }

                if (value is Type)
                {
                    writer.Write(Token_Type);
                    SerializeType(writer, (Type)value);
                    continue;
                }

                var valueType = value.GetType();

                if (value is Array)
                {
                    // We only support Arrays with rank 1 (No multi dimensional arrays
                    if (((Array)value).Rank > 1)
                    {
                        continue;
                    }

                    var underlyingType = valueType.GetElementType();

                    if (underlyingType == typeof(string))
                    {
                        var strings = (string[])value;
                        var containsNulls = false;
                        for (var i = 0; i < strings.Length; i++)
                        {
                            if (strings[i] == null)
                            {
                                // Will have to treat these as generic arrays since we
                                // can't represent nulls in the binary stream, without
                                // writing out string token markers.
                                // Generic array writing includes the token markers.
                                containsNulls = true;
                                break;
                            }
                        }

                        if (!containsNulls)
                        {
                            writer.Write(Token_StringArray);
                            writer.Write7BitEncodedInt(strings.Length);
                            for (var i = 0; i < strings.Length; i++)
                            {
                                writer.Write(strings[i]);
                            }
                            continue;
                        }
                    }

                    var values = (Array)value;

                    // Optimize for sparse arrays, if the array is more than 3/4 nulls
                    if (values.Length > 3)
                    {
                        var sparseThreshold = (values.Length / 4) + 1;
                        var numValues = 0;
                        var items = new List<int>(sparseThreshold);
                        for (var i = 0; i < values.Length; ++i)
                        {
                            if (values.GetValue(i) != null)
                            {
                                ++numValues;
                                if (numValues >= sparseThreshold)
                                {
                                    break;
                                }
                                items.Add(i);
                            }
                        }

                        // We have enough nulls to use sparse array format <index, value, index, value, ...>
                        if (numValues < sparseThreshold)
                        {
                            writer.Write(Token_SparseArray);
                            SerializeType(writer, underlyingType);

                            writer.Write7BitEncodedInt(values.Length);
                            writer.Write7BitEncodedInt(numValues);

                            // Now we need to just serialize pairs representing the index, and the item
                            foreach (var index in items)
                            {
                                writer.Write7BitEncodedInt(index);
                                SerializeValue(writer, values.GetValue(index));
                            }

                            continue;
                        }
                    }

                    writer.Write(Token_Array);
                    SerializeType(writer, underlyingType);

                    writer.Write7BitEncodedInt(values.Length);
                    for (var i = values.Length - 1; i >= 0; i--)
                    {
                        objectStack.Push(values.GetValue(i));
                    }

                    continue;
                }

                if (valueType.IsEnum)
                {
                    var underlyingType = Enum.GetUnderlyingType(valueType);
                    if (underlyingType == typeof(int))
                    {
                        writer.Write(Token_IntEnum);
                        SerializeType(writer, valueType);
                        writer.Write7BitEncodedInt((int)value);

                        continue;
                    }
                }

                if (valueType == typeof(Color))
                {
                    var c = (Color)value;
                    if (c.IsEmpty)
                    {
                        writer.Write(Token_EmptyColor);
                        continue;
                    }
                    if (!c.IsNamedColor)
                    {
                        writer.Write(Token_Color);
                        writer.Write(c.ToArgb());
                        continue;
                    }
                    else
                    {
                        writer.Write(Token_KnownColor);
                        writer.Write7BitEncodedInt((int)c.ToKnownColor());
                        continue;
                    }
                }

                if (value is Unit)
                {
                    var uval = (Unit)value;
                    if (uval.IsEmpty)
                    {
                        writer.Write(Token_EmptyUnit);
                    }
                    else
                    {
                        writer.Write(Token_Unit);
                        writer.Write(uval.Value);
                        writer.Write((int)uval.Type);
                    }
                    continue;
                }

                // Handle the remaining types
                // First try to get a type converter, and then resort to
                // binary serialization if all else fails

                var converter = TypeDescriptor.GetConverter(valueType);
                bool canConvert = System.Web.UI.Util.CanConvertToFrom(converter, typeof(string));

                if (canConvert)
                {
                    writer.Write(Token_StringFormatted);
                    SerializeType(writer, valueType);
                    writer.Write(converter.ConvertToInvariantString(null, value));
                }
                else
                {
                    // TODO: Migration: BinaryFormatter
                    IFormatter formatter = new BinaryFormatter();
                    MemoryStream ms = new MemoryStream(256);
                    formatter.Serialize(ms, value);

                    byte[] buffer = ms.GetBuffer();
                    int length = (int)ms.Length;

                    writer.Write(Token_BinarySerialized);
                    // writer.WriteEncoded(length);
                    writer.Write7BitEncodedInt(length);
                    if (buffer.Length != 0)
                    {
                        writer.Write(buffer, 0, (int)length);
                    }
                    // throw new InvalidOperationException($"Unsupported type {valueType.FullName}");
                }
            }
            while (objectStack.Count > 0);
        }
        catch (Exception serializationException)
        {
            if (value != null)
            {
                throw new ArgumentException(SR.GetString(SR.ErrorSerializingValue, value.ToString(), value.GetType().FullName),
                                        serializationException);
            }

            throw;
        }
    }

    #region Implementation of IStateFormatter
    object IStateFormatter.Deserialize(string serializedState) => Deserialize(serializedState);

    string IStateFormatter.Serialize(object state) => Serialize(state);
    #endregion

    #region IStateFormatter2 Members
    object IStateFormatter2.Deserialize(string serializedState, Purpose purpose) => Deserialize(serializedState, purpose);

    string IStateFormatter2.Serialize(object state, Purpose purpose) => Serialize(state, purpose);
    #endregion
}

