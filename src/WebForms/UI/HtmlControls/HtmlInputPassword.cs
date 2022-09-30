// MIT License.

/*
 * HtmlInputPassword.cs
 *
 * Copyright (c) 2000 Microsoft Corporation
 */

namespace System.Web.UI.HtmlControls;

using System.ComponentModel;
using System.Web.UI;

/// <devdoc>
///    <para>
///       The <see langword='HtmlInputPassword'/>
///       class defines the methods, properties, and events for the HtmlInputPassword server
///       control. This class allows programmatic access to the HTML &lt;input type=
///       text&gt;
///       and &lt;input type=
///       password&gt; elements on the server.
///    </para>
/// </devdoc>
[
DefaultEvent("ServerChange"),
ValidationProperty("Value"),
SupportsEventValidation,
]
public class HtmlInputPassword : HtmlInputText, IPostBackDataHandler
{

    /*
     * Creates an intrinsic Html INPUT type=password control.
     */

    public HtmlInputPassword() : base("password")
    {
    }

    protected override void RenderAttributes(HtmlTextWriter writer)
    {
        // Remove value from viewstate for input type=password
        ViewState.Remove("value");

        base.RenderAttributes(writer);
    }
}
