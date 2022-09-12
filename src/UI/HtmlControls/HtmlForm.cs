// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web.UI.HtmlControls;

public class HtmlForm : HtmlContainerControl
{
    public HtmlForm()
        : base("form")
    {
    }

    public string Action
    {
        get => Attributes["action"] ?? string.Empty;
        set => Attributes["action"] = MapStringAttributeToString(value);
    }

    public string Method
    {
        get => Attributes["method"] ?? "post";
        set => Attributes["method"] = MapStringAttributeToString(value);
    }

    protected internal override void RenderChildren(HtmlTextWriter writer)
    {
        var page = Page;

        if (page is not null)
        {
            Page.BeginFormRender(writer, UniqueID);
        }
    }

    protected override void RenderAttributes(HtmlTextWriter writer)
    {
        writer.WriteAttribute("method", Method);
        Attributes.Remove("method");

        writer.WriteAttribute("action", GetActionAttribute(), true);
        Attributes.Remove("action");

        EnsureId();

        base.RenderAttributes(writer);
    }

    private static void EnsureId()
    {
    }

    private string GetActionAttribute()
    {
        if (Action is { Length: > 0 } action)
        {
            return action;
        }

        return Context.Request.Path ?? "/";
    }
}

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
        get => string.Equals("disabled", Attributes["disabled"]);
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
