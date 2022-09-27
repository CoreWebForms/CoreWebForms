// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

/*
 */

namespace System.Web.UI.WebControls;

using System;

/// <devdoc>
///    <para>Specifies the type of the item in a list.</para>
/// </devdoc>
[Flags]
public enum DataControlRowState
{

    /// <devdoc>
    ///    <para> 
    ///       An item. It is databound.</para>
    /// </devdoc>
    Normal = 0,

    /// <devdoc>
    ///    <para> 
    ///       An alternate (even-indexed) item. It is databound.</para>
    /// </devdoc>
    Alternate = 1,

    /// <devdoc>
    ///    <para> 
    ///       The selected item. It is databound.</para>
    /// </devdoc>
    Selected = 2,

    /// <devdoc>
    ///    <para> 
    ///       The item in edit mode. It is databound.</para>
    /// </devdoc>
    Edit = 4,

    /// <devdoc>
    ///    <para> 
    ///       The item in insert mode. It is databound.</para>
    /// </devdoc>
    Insert = 8
}

