// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using System.Collections;
using System.Web.Util;

namespace System.Web.UI;

public sealed class AttributeCollection
{
    private readonly StateBag _bag;
    private CssStyleCollection? _styleColl;

    public AttributeCollection(StateBag bag)
    {
        _bag = bag;
    }

    public string? this[string key]
    {
        get
        {
            return _styleColl != null && StringUtil.EqualsIgnoreCase(key, "style") ? _styleColl.Value : _bag[key] as string;
        }
        set => Add(key, value);
    }

    public ICollection Keys => _bag.Keys;

    public int Count => _bag.Count;

    public CssStyleCollection CssStyle => _styleColl ??= new CssStyleCollection(_bag);

    public void Add(string key, string? value)
    {
        if (_styleColl != null && StringUtil.EqualsIgnoreCase(key, "style"))
        {
            _styleColl.Value = value;
        }
        else
        {
            _bag[key] = value;
        }
    }

    public override bool Equals(object? o)
    {
        if (o is AttributeCollection attrs)
        {
            if (attrs.Count != _bag.Count)
            {
                return false;
            }
            foreach (DictionaryEntry attr in _bag)
            {
                if (this[(string)attr.Key] != attrs[(string)attr.Key])
                {
                    return false;
                }
            }
            return true;
        }
        return false;
    }

    public override int GetHashCode()
    {
        var code = new HashCode();

        foreach (DictionaryEntry attr in _bag)
        {
            code.Add(attr.Key);
            code.Add(attr.Value);
        }

        return code.ToHashCode();
    }

    public void Remove(string key)
    {
        if (_styleColl != null && StringUtil.EqualsIgnoreCase(key, "style"))
        {
            _styleColl.Clear();
        }
        else
        {
            _bag.Remove(key);
        }
    }

    public void Clear()
    {
        _bag.Clear();
        if (_styleColl != null)
        {
            _styleColl.Clear();
        }
    }

    public void Render(HtmlTextWriter writer)
    {
        if (_bag.Count > 0)
        {
            IDictionaryEnumerator e = _bag.GetEnumerator();

            while (e.MoveNext())
            {
                if (e.Value is StateItem item)
                {
                    if (e.Key is string key && item.Value is string value)
                    {
                        writer.WriteAttribute(key, value, fEncode: true);
                    }
                }
            }
        }
    }

    public void AddAttributes(HtmlTextWriter writer)
    {
        if (_bag.Count > 0)
        {
            var e = _bag.GetEnumerator();

            while (e.MoveNext())
            {
                if (e.Value is StateItem item)
                {
                    if (e.Key is string key && item.Value is string value)
                    {
                        writer.AddAttribute(key, value, fEndode: true);
                    }
                }
            }
        }
    }
}
