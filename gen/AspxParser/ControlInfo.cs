// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.SystemWebAdapters.UI.PageParser;

public class ControlInfo
{
    private HashSet<string> _events;
    private HashSet<string> _strings;
    private HashSet<string> _other;

    public ControlInfo(string ns, string name)
    {
        Name = name;
    }

    public string Name { get; }

    public string DefaultProperty { get; set; }

    public string ValidationProperty { get; set; }

    public string DefaultEvent { get; set; }

    public bool SupportsEventValidation { get; set; }

    public ICollection<string> Events => _events ??= new HashSet<string>();

    public ICollection<string> Strings => _strings ??= new HashSet<string>();

    public ICollection<string> Other => _other ??= new HashSet<string>();

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

    public (DataType, string Key) GetDataType(string name)
    {
        if (_events?.Contains(name) == true || string.Equals("OnClick", name, StringComparison.OrdinalIgnoreCase))
        {
            return (DataType.Delegate, Normalize(name, "OnClick", DefaultEvent));
        }

        if (_strings?.Contains(name) == true || string.Equals("Value", name, StringComparison.OrdinalIgnoreCase))
        {
            return (DataType.String, Normalize(name, "Value", DefaultProperty));
        }

        if (_other?.Contains(name) == true || string.Equals("Value", name, StringComparison.OrdinalIgnoreCase))
        {
            return (DataType.NoQuotes, Normalize(name, "Value", DefaultProperty));
        }

        return (DataType.None, name);
    }
}
