// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

namespace System.Web.UI;
/// <devdoc>
///    <para> Allows a control to specify that it needs a
///       tag name in its constructor.</para>
/// </devdoc>
[AttributeUsage(AttributeTargets.Class)]
public sealed class ConstructorNeedsTagAttribute : Attribute
{
    /// <devdoc>
    /// <para>Initializes a new instance of the <see cref='System.Web.UI.ConstructorNeedsTagAttribute'/> class.</para>
    /// </devdoc>
    public ConstructorNeedsTagAttribute()
    {
    }

    /// <devdoc>
    /// <para>Initializes a new instance of the <see cref='System.Web.UI.ConstructorNeedsTagAttribute'/> class.</para>
    /// </devdoc>
    public ConstructorNeedsTagAttribute(bool needsTag)
    {
        NeedsTag = needsTag;
    }

    /// <devdoc>
    ///    <para>Indicates whether a control needs a tag in its contstructor. This property is read-only.</para>
    /// </devdoc>
    public bool NeedsTag { get; }
}
