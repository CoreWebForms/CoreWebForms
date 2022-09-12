// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

/*
 */

namespace System.Web.UI.WebControls;

using System.ComponentModel;

/// <devdoc>
///    <para>
///       Specifies the horizonal alignment.
///    </para>
/// </devdoc>
[TypeConverterAttribute(typeof(HorizontalAlignConverter))]
public enum HorizontalAlign
{

    /// <devdoc>
    ///    <para>
    ///       Specifies that horizonal alignment is not set.
    ///    </para>
    /// </devdoc>
    NotSet = 0,

    /// <devdoc>
    ///    <para>
    ///       Specifies that horizonal alignment is left justified.
    ///    </para>
    /// </devdoc>
    Left = 1,

    /// <devdoc>
    ///    <para>
    ///       Specifies that horizonal alignment is centered.
    ///    </para>
    /// </devdoc>
    Center = 2,

    /// <devdoc>
    ///    <para>
    ///       Specifies that horizonal alignment is right justified.
    ///    </para>
    /// </devdoc>
    Right = 3,

    /// <devdoc>
    ///    <para>
    ///       Specifies that horizonal alignment is justified.
    ///    </para>
    /// </devdoc>
    Justify = 4
}

