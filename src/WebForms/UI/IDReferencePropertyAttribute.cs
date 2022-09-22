// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

namespace System.Web.UI;

using System;
using System.Diagnostics.CodeAnalysis;

/// <devdoc>
/// An IDReferencePropertyAttribute metadata attribute can be applied to string properties
/// that contain ID references.
/// This can be used to identify ID reference properties which allows design-time functionality 
/// to do interesting things with the property values.
/// </devdoc>
[AttributeUsage(AttributeTargets.Property)]
public sealed class IDReferencePropertyAttribute : Attribute
{
    /// <devdoc>
    /// </devdoc>
    public IDReferencePropertyAttribute()
        : this(typeof(Control))
    {
    }

    /// <devdoc>
    /// Used to mark a property as an ID reference. In addition, the type of controls
    /// can be specified.
    /// </devdoc>
    public IDReferencePropertyAttribute(Type referencedControlType)
    {
        ReferencedControlType = referencedControlType;
    }

    /// <devdoc>
    /// The types of controls allowed by the property.
    /// </devdoc>
    public Type ReferencedControlType { get; }

    /// <internalonly/>
    public override int GetHashCode() => ReferencedControlType?.GetHashCode() ?? 0;

    /// <internalonly/>
    public override bool Equals(object obj)
    {
        if (obj == this)
        {
            return true;
        }

        if (obj is IDReferencePropertyAttribute other)
        {
            return ReferencedControlType == other.ReferencedControlType;
        }

        return false;
    }
}
