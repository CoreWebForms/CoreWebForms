// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web.UI.WebControls;

public class WebControl : Control
{
    public WebControl()
        : this(HtmlTextWriterTag.Span)
    {
    }

    public WebControl(HtmlTextWriterTag tag)
    {
    }
}
