// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

/*
 */

namespace System.Web.UI;

/// <devdoc>
///    <para>Identifies the validation property for a component.</para>
/// </devdoc>
[AttributeUsage(AttributeTargets.Class)]
public sealed class ValidationPropertyAttribute : Attribute
{

    /// <devdoc>
    ///  This is the validation event name.
    /// </devdoc>
    private readonly string name;

    /// <devdoc>
    /// <para>Initializes a new instance of the <see cref='System.Web.UI.ValidationPropertyAttribute'/> class.</para>
    /// </devdoc>
    public ValidationPropertyAttribute(string name)
    {
        this.name = name;
    }

    /// <devdoc>
    ///    <para>Indicates the name the specified validation attribute. This property is 
    ///       read-only.</para>
    /// </devdoc>
    public string Name => name;
}

