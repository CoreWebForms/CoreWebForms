// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

/*
 * Classes related to templated control support
 *
 * Copyright (c) 1999 Microsoft Corporation
 */
namespace System.Web.UI;

using System;
using System.ComponentModel;
/*
 * This class defines the TemplateAttribute attribute that can be placed on
 * properties of type ITemplate.  It allows the parser to strongly type the
 * container, which makes it easier to write render code in a template
 */

/// <devdoc>
///    <para>[To be supplied.]</para>
/// </devdoc>
[AttributeUsage(AttributeTargets.Property)]
public sealed class TemplateContainerAttribute : Attribute
{
    private readonly Type _containerType;

    private readonly BindingDirection _bindingDirection;

    /// <devdoc>
    /// <para>Whether the template supports two-way binding.</para>
    /// </devdoc>
    public BindingDirection BindingDirection
    {
        get
        {
            return _bindingDirection;
        }
    }

    /// <devdoc>
    ///    <para></para>
    /// </devdoc>
    public Type ContainerType
    {
        get
        {
            return _containerType;
        }
    }

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public TemplateContainerAttribute(Type containerType) : this(containerType, BindingDirection.OneWay)
    {
    }

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public TemplateContainerAttribute(Type containerType, BindingDirection bindingDirection)
    {
        _containerType = containerType;
        _bindingDirection = bindingDirection;
    }
}
