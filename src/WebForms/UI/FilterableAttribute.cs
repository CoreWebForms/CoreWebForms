// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.ComponentModel;

#pragma warning disable CA1810 // Initialize reference type static fields inline

#nullable disable

namespace System.Web.UI;
/// <devdoc>
/// <para></para>
/// </devdoc>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
public sealed class FilterableAttribute : Attribute
{
    /// <internalonly/>
    /// <devdoc>
    /// <para></para>
    /// </devdoc>
    public static readonly FilterableAttribute Yes = new FilterableAttribute(true);

    /// <internalonly/>
    /// <devdoc>
    /// <para></para>
    /// </devdoc>
    public static readonly FilterableAttribute No = new FilterableAttribute(false);

    /// <internalonly/>
    /// <devdoc>
    /// <para></para>
    /// </devdoc>
    public static readonly FilterableAttribute Default = Yes;
    private static readonly Hashtable _filterableTypes;

    static FilterableAttribute()
    {
        // Create a synchronized wrapper
        _filterableTypes = Hashtable.Synchronized(new Hashtable());
    }

    /// <devdoc>
    /// </devdoc>
    public FilterableAttribute(bool filterable)
    {
        Filterable = filterable;
    }

    /// <devdoc>
    ///    <para> Indicates if the property is Filterable.</para>
    /// </devdoc>
    public bool Filterable { get; }

    /// <internalonly/>
    public override bool Equals(object obj)
    {
        if (obj == this)
        {
            return true;
        }

        return (obj is FilterableAttribute other) && (other.Filterable == Filterable);
    }

    /// <internalonly/>
    public override int GetHashCode() => Filterable.GetHashCode();

    /// <internalonly/>
    public override bool IsDefaultAttribute() => Equals(Default);

    public static bool IsObjectFilterable(object instance)
    {
        if (instance == null)
        {
            throw new ArgumentNullException(nameof(instance));
        }

        return IsTypeFilterable(instance.GetType());
    }

    public static bool IsPropertyFilterable(PropertyDescriptor propertyDescriptor)
    {
        if (propertyDescriptor.Attributes[typeof(FilterableAttribute)] is FilterableAttribute filterableAttr)
        {
            return filterableAttr.Filterable;
        }

        return true;
    }

    public static bool IsTypeFilterable(Type type)
    {
        if (type == null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        object result = _filterableTypes[type];
        if (result != null)
        {
            return (bool)result;
        }

        System.ComponentModel.AttributeCollection attrs = TypeDescriptor.GetAttributes(type);
        FilterableAttribute attr = (FilterableAttribute)attrs[typeof(FilterableAttribute)];
        result = (attr != null) && attr.Filterable;
        _filterableTypes[type] = result;

        return (bool)result;
    }
}

