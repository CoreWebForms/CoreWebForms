// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

/*
 */

namespace System.Web.UI.WebControls;
/// <devdoc>
///    <para>Specifies the type of the item in a list.</para>
/// </devdoc>
public enum DataControlCellType
{

    /// <devdoc>
    ///    <para> 
    ///       A header. It is not databound.</para>
    /// </devdoc>
    Header = 0,

    /// <devdoc>
    ///    <para> 
    ///       A footer. It is not databound.</para>
    /// </devdoc>
    Footer = 1,

    /// <devdoc>
    ///    An item. It is databound.
    /// </devdoc>
    DataCell = 2
}

