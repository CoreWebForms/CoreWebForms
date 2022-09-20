// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Web.UI.Features;

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
        if (GetHierarchicalFeature<IFormWriterFeature>() is { } form)
        {
            form.OnFormRender();
            form.BeginFormRender(writer, UniqueID);

            base.RenderChildren(writer);

            form.EndFormRender(writer, UniqueID);
            form.OnFormPostRender(writer);
        }
        else
        {
            base.RenderChildren(writer);
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
