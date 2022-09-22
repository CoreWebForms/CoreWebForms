// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Compilation;
using System.Web.Util;

namespace System.Web.Configuration;
public sealed class CodeSubDirectoriesCollection : ConfigurationElementCollection
{
    private static ConfigurationPropertyCollection _properties;
    private bool _didRuntimeValidation;

    static CodeSubDirectoriesCollection()
    {
        // Property initialization
        _properties = new ConfigurationPropertyCollection();
    }
    public CodeSubDirectoriesCollection()
        : base(StringComparer.OrdinalIgnoreCase)
    {
    }

    protected override ConfigurationPropertyCollection Properties
    {
        get
        {
            return _properties;
        }
    }

    public CodeSubDirectory this[int index]
    {
        get
        {
            return (CodeSubDirectory)BaseGet(index);
        }
        set
        {
            if (BaseGet(index) != null)
            {
                BaseRemoveAt(index);
            }
            BaseAdd(index, value);
        }
    }

    public void Add(CodeSubDirectory codeSubDirectory)
    {
        BaseAdd(codeSubDirectory);
    }

    public void Clear()
    {
        BaseClear();
    }

    public void Remove(string directoryName)
    {
        BaseRemove(directoryName);
    }

    public void RemoveAt(int index)
    {
        BaseRemoveAt(index);
    }


    public override ConfigurationElementCollectionType CollectionType
    {
        get
        {
            return ConfigurationElementCollectionType.BasicMap;
        }
    }

    protected override ConfigurationElement CreateNewElement()
    {
        return new CodeSubDirectory();
    }

    protected override string ElementName
    {
        get
        {
            return "add";
        }
    }

    protected override Object GetElementKey(ConfigurationElement element)
    {
        return ((CodeSubDirectory)element).DirectoryName;
    }

    // Validate the element for runtime use
    internal void EnsureRuntimeValidation()
    {
        if (_didRuntimeValidation)
        {
            return;
        }

        foreach (CodeSubDirectory subDir in this)
        {
            subDir.DoRuntimeValidation();
        }

        _didRuntimeValidation = true;
    }
}
