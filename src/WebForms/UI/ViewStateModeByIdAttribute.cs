// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.Collections;
using System.ComponentModel;

namespace System.Web.UI;

[AttributeUsage(AttributeTargets.Class)]
public sealed class ViewStateModeByIdAttribute : Attribute
{
    private static readonly Hashtable _viewStateIdTypes = Hashtable.Synchronized(new Hashtable());

    public ViewStateModeByIdAttribute()
    {
    }

    internal static bool IsEnabled(Type type)
    {
        if (!_viewStateIdTypes.ContainsKey(type))
        {
            System.ComponentModel.AttributeCollection attrs = TypeDescriptor.GetAttributes(type);
            ViewStateModeByIdAttribute attr = (ViewStateModeByIdAttribute)attrs[typeof(ViewStateModeByIdAttribute)];
            _viewStateIdTypes[type] = (attr != null);
        }
        return (bool)_viewStateIdTypes[type];
    }
}

