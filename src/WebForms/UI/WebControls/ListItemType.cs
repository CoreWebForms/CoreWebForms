// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

/*
 */

namespace System.Web.UI.WebControls;
/// <devdoc>
///    <para>Specifies the type of the item in a list.</para>
/// </devdoc>
public enum ListItemType
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
    Item = 2,

    /// <devdoc>
    ///    <para> 
    ///       An alternating (even-indexed) item. It is databound.</para>
    /// </devdoc>
    AlternatingItem = 3,

    /// <devdoc>
    ///    <para> 
    ///       The selected item. It is databound.</para>
    /// </devdoc>
    SelectedItem = 4,

    /// <devdoc>
    ///    <para> 
    ///       The item in edit mode. It is databound.</para>
    /// </devdoc>
    EditItem = 5,

    /// <devdoc>
    ///    <para> A separator. It is not databound.</para>
    /// </devdoc>
    Separator = 6,

    /// <devdoc>
    ///    <para> A pager. It is used for rendering paging (page accessing) UI associated 
    ///       with the <see cref='System.Web.UI.WebControls.DataGrid'/> control and is not
    ///       databound.</para>
    /// </devdoc>
    Pager = 7
}

