// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web.UI.HtmlControls;

public abstract class HtmlControl : Control
{
    private AttributeCollection? _attributes;

    public HtmlControl(string tag)
    {
        TagName = tag;
    }

    public AttributeCollection Attributes => _attributes ??= new(ViewState);

    public CssStyleCollection Style => Attributes.CssStyle;

    public bool Disabled
    {
        get => Attributes["disabled"] is { } disabled ? string.Equals("disabled", disabled) : false;
        set => Attributes["disabled"] = value ? "disabled" : null;
    }

    protected internal override void Render(HtmlTextWriter writer)
    {
        RenderBeginTag(writer);
    }

    protected virtual void RenderAttributes(HtmlTextWriter writer)
    {
        Attributes.Render(writer);
    }

    protected virtual void RenderBeginTag(HtmlTextWriter writer)
    {
        writer.WriteBeginTag(TagName);
        RenderAttributes(writer);
        writer.Write(HtmlTextWriter.TagRightChar);
    }

    public string TagName { get; }

    internal static string? MapStringAttributeToString(string s)
        => string.IsNullOrEmpty(s) ? null : s;
}
