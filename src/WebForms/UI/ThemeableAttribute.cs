// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.Collections;

namespace System.Web.UI;

#pragma warning disable CA1810 // Initialize reference type static fields inline

/// <devdoc>
/// <para></para>
/// </devdoc>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
public sealed class ThemeableAttribute : Attribute
{
    /// <internalonly/>
    /// <devdoc>
    /// <para></para>
    /// </devdoc>
    public static readonly ThemeableAttribute Yes = new ThemeableAttribute(true);

    /// <internalonly/>
    /// <devdoc>
    /// <para></para>
    /// </devdoc>
    public static readonly ThemeableAttribute No = new ThemeableAttribute(false);

    /// <internalonly/>
    /// <devdoc>
    /// <para></para>
    /// </devdoc>
    public static readonly ThemeableAttribute Default = Yes;
    private static readonly Hashtable _themeableTypes;

    static ThemeableAttribute()
    {
        // Create a synchronized wrapper
        _themeableTypes = Hashtable.Synchronized(new Hashtable());
    }

    /// <devdoc>
    /// </devdoc>
    public ThemeableAttribute(bool themeable)
    {
        Themeable = themeable;
    }

    /// <devdoc>
    ///    <para> Indicates if the property is themeable.</para>
    /// </devdoc>
    public bool Themeable { get; }

    /// <internalonly/>
    public override bool Equals(object obj)
    {
        if (obj == this)
        {
            return true;
        }

        ThemeableAttribute other = obj as ThemeableAttribute;
        return (other != null) && (other.Themeable == Themeable);
    }

    /// <internalonly/>
    public override int GetHashCode()
    {
        return Themeable.GetHashCode();
    }

    /// <internalonly/>
    public override bool IsDefaultAttribute()
    {
        return this.Equals(Default);
    }

    public static bool IsObjectThemeable(object instance)
    {
        return instance == null ? throw new ArgumentNullException(nameof(instance)) : IsTypeThemeable(instance.GetType());
    }

    public static bool IsTypeThemeable(Type type)
    {
        if (type == null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        object result = _themeableTypes[type];
        if (result != null)
        {
            return (bool)result;
        }

        //System.ComponentModel.AttributeCollection attrs = TypeDescriptor.GetAttributes(type);
        //ThemeableAttribute attr = (ThemeableAttribute)attrs[typeof(ThemeableAttribute)];
        ThemeableAttribute attr = Attribute.GetCustomAttribute(type, typeof(ThemeableAttribute)) as ThemeableAttribute;
        result = (attr != null) && attr.Themeable;
        _themeableTypes[type] = result;

        return (bool)result;
    }
}

