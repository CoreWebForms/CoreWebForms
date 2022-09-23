// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web.UI.WebControls;

/// <devdoc>
///    <para>
///       Specifies the gridline style.
///    </para>
/// </devdoc>
public enum GridLines
{

    /// <devdoc>
    ///    <para>
    ///       A grid with no grid lines rendered.
    ///    </para>
    /// </devdoc>
    None = 0,

    /// <devdoc>
    ///    <para>
    ///       A grid with only horizontal grid lines rendered.
    ///    </para>
    /// </devdoc>
    Horizontal = 1,

    /// <devdoc>
    ///    <para>
    ///       A grid with only vertical grid lines rendered.
    ///    </para>
    /// </devdoc>
    Vertical = 2,

    /// <devdoc>
    ///    <para>
    ///       A grid woth both horizontal and vertical grid lines rendered.
    ///    </para>
    /// </devdoc>
    Both = 3
}

