// MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.SystemWebAdapters.Compiler;

public class ControlInfo
{
    private HashSet<string> _events;
    private HashSet<string> _strings;
    private HashSet<string> _other;

    private readonly Dictionary<string, DataType> _types = new();

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

    private string Normalize(string name, string match, string defaultName)
    {
        if (defaultName is null)
        {
            return name;
        }

        if (string.Equals(name, match, StringComparison.OrdinalIgnoreCase))
        {
            return defaultName;
        }

        return name;
    }

    public void AddProperty(string name, DataType type)
        => _types.Add(name, type);

    public (DataType, string Key) GetDataType(string name)
    {
        if (_types.TryGetValue(name, out var type))
        {
            if (type == DataType.Delegate || string.Equals("OnClick", name, StringComparison.OrdinalIgnoreCase))
            {
                return (DataType.Delegate, Normalize(name, "OnClick", DefaultEvent));
            }

            if (type == DataType.String || string.Equals("Value", name, StringComparison.OrdinalIgnoreCase))
            {
                return (DataType.String, Normalize(name, "Value", DefaultProperty));
            }

            if (type == DataType.NoQuotes || string.Equals("Value", name, StringComparison.OrdinalIgnoreCase))
            {
                return (DataType.NoQuotes, Normalize(name, "Value", DefaultProperty));
            }

            return (type, name);
        }

        return (DataType.None, name);
    }
}
