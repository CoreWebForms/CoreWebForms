// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web.UI.HtmlControls;

public abstract class HtmlContainerControl : HtmlControl
{
    public HtmlContainerControl(string tag)
        : base(tag)
    {
    }

    protected internal override void Render(HtmlTextWriter writer)
    {
        RenderBeginTag(writer);
        RenderChildren(writer);
        RenderEndTag(writer);
    }

    protected virtual void RenderEndTag(HtmlTextWriter writer)
    {
        writer.WriteEndTag(TagName);
    }
}
