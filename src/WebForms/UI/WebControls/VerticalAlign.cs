// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;

/*
 */

namespace System.Web.UI.WebControls;
/// <devdoc>
///    <para>
///       Specifies the vertical alignment of an object or text within a control.
///    </para>
/// </devdoc>
[TypeConverterAttribute(typeof(VerticalAlignConverter))]
public enum VerticalAlign
{

    /// <devdoc>
    ///    <para>
    ///       Vertical
    ///       alignment property is not set.
    ///    </para>
    /// </devdoc>
    NotSet = 0,

    /// <devdoc>
    ///    <para>
    ///       The object or text is aligned with the top of the
    ///       enclosing control.
    ///    </para>
    /// </devdoc>
    Top = 1,

    /// <devdoc>
    ///    <para>
    ///       The object or text is placed
    ///       across the vertical center of the enclosing control.
    ///    </para>
    /// </devdoc>
    Middle = 2,

    /// <devdoc>
    ///    <para>
    ///       The object or text is aligned with the bottom of the enclosing
    ///       control.
    ///    </para>
    /// </devdoc>
    Bottom = 3
}
