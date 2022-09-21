// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;

namespace System.Web.UI.Features;

internal class ViewStateSerializer : IViewStateSerializer
{
    private readonly Dictionary<Type, int> _fromType = new();
    private readonly List<TypeConverter> _list = new();

    public object? Deserialize(BinaryReader reader)
    {
        var converter = GetConverter(reader.Read7BitEncodedInt());

        return converter.ConvertFromInvariantString(reader.ReadString());
    }

    public void Serialize(BinaryWriter writer, object obj)
    {
        var (converter, index) = GetConverter(obj.GetType());

        writer.Write7BitEncodedInt(index);
        writer.Write(converter.ConvertToInvariantString(obj) ?? string.Empty);
    }

    private (TypeConverter, int) GetConverter(Type type)
    {
        lock (_fromType)
        {
            if (_fromType.TryGetValue(type, out var result))
            {
                return (_list[result], result);
            }

            var converter = TypeDescriptor.GetConverter(type);
            var index = _list.Count;
            _list.Add(converter);
            _fromType.Add(type, index);
            return (converter, index);
        }
    }

    private TypeConverter GetConverter(int index)
    {
        lock (_fromType)
        {
            return index < _list.Count ? _list[index] : throw new InvalidOperationException("Unknown view state type");
        }
    }
}
