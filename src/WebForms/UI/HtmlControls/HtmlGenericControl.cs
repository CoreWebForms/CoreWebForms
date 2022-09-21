// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;

#nullable disable

namespace System.Web.UI.HtmlControls;
/*
 *  A control representing an unknown Html tag.
 */

/// <devdoc>
///    <para>
///       The <see langword='HtmlGenericControl'/> class defines the methods,
///       properties, and events for all HTML Server control tags not represented by a
///       specific class.
///    </para>
/// </devdoc>
[ConstructorNeedsTag(true)]
public class HtmlGenericControl : HtmlContainerControl
{
    /*
     * Creates a new WebControl
     */

    /// <devdoc>
    /// <para>Initializes a new instance of the <see cref='System.Web.UI.HtmlControls.HtmlGenericControl'/> class with default 
    ///    values.</para>
    /// </devdoc>
    public HtmlGenericControl() : this("span")
    {
    }

    /*
     *  Creates a new HtmlGenericControl
     */

    /// <devdoc>
    /// <para>Initializes a new instance of the <see cref='System.Web.UI.HtmlControls.HtmlGenericControl'/> class using the specified 
    ///    string.</para>
    /// </devdoc>
    public HtmlGenericControl(string tag)
    {
        _tagName = tag ?? string.Empty;
    }

    /*
    * Property to get name of tag.
    */

    /// <devdoc>
    ///    <para>
    ///       Gets or sets the element name of a tag that contains a
    ///       runat="server" attribute/value pair.
    ///    </para>
    /// </devdoc>
    [
    WebCategory("Appearance"),
    DefaultValue(""),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
    ]
    public new string TagName
    {
        get => _tagName;
        set => _tagName = value;
    }
}
