// MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.SystemWebAdapters.Compiler;

public class ControlInfo
{
    private readonly Dictionary<string, DataType> _types = new();
    private readonly Dictionary<string, string> _enums = new();

    public ControlInfo(string ns, string name)
    {
        Namespace = ns;
        Name = name;

        QName = new(ns, name);
    }

    internal QName QName { get; }

    public string Namespace { get; }

    public string Name { get; }

    public string DefaultProperty { get; set; }

    public string ValidationProperty { get; set; }

    public string DefaultEvent { get; set; }

    public bool ChildrenAsProperties { get; set; }

    public bool SupportsEventValidation { get; set; }

    public void AddProperty(string name, DataType type)
    {
        if (!_types.ContainsKey(name))
        {
            _types.Add(name, type);
        }
    }

    public void AddEnum(string propertyName, Type @enum)
    {
        AddProperty(propertyName, DataType.Enum);
        _enums.Add(propertyName, $"{@enum.Namespace}.{@enum.Name}");
    }

    private string Normalize(string name)
    {
        if (string.Equals(name, "OnClick", StringComparison.OrdinalIgnoreCase))
        {
            return DefaultEvent ?? name;
        }

        if (string.Equals(name, "Value", StringComparison.OrdinalIgnoreCase))
        {
            return DefaultProperty ?? name;
        }

        return name;
    }

    public (DataType, string Key) GetDataType(string name)
    {
        var normalized = Normalize(name);

        if (_types.TryGetValue(normalized, out var type))
        {
            if (type == DataType.Enum)
            {
                return (type, _enums[name]);
            }

            return (type, normalized);
        }

        return (DataType.None, name);
    }
}
