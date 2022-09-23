// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web.UI;

/// <devdoc>
///  Used to access data bindings of a Control.
///  Only valid for use at design-time.
/// </devdoc>
public interface IDataBindingsAccessor
{

    /// <devdoc>
    ///    <para>Indicates a collection of all data bindings on the control. This property is 
    ///       read-only.</para>
    /// </devdoc>
    DataBindingCollection DataBindings
    {
        get;
    }

    /// <devdoc>
    ///    <para>Returns whether the control contains any data binding logic. This method is 
    ///       only accessed by RAD designers.</para>
    /// </devdoc>
    bool HasDataBindings
    {
        get;
    }
}

