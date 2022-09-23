// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;

namespace System.Web.UI.HtmlControls;

public class HtmlMeta : HtmlControl
{

    public HtmlMeta() : base("meta")
    {
    }

    [
    WebCategory("Appearance"),
    DefaultValue(""),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
    ]
    public virtual string Content
    {
        get
        {
            string s = Attributes["content"];
            return ((s != null) ? s : String.Empty);
        }
        set
        {
            Attributes["content"] = MapStringAttributeToString(value);
        }
    }

    [
    WebCategory("Appearance"),
    DefaultValue(""),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
    ]
    public virtual string HttpEquiv
    {
        get
        {
            string s = Attributes["http-equiv"];
            return ((s != null) ? s : String.Empty);
        }
        set
        {
            Attributes["http-equiv"] = MapStringAttributeToString(value);
        }
    }

    [
    WebCategory("Appearance"),
    DefaultValue(""),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
    ]
    public virtual string Name
    {
        get
        {
            string s = Attributes["name"];
            return ((s != null) ? s : String.Empty);
        }
        set
        {
            Attributes["name"] = MapStringAttributeToString(value);
        }
    }

    [
    WebCategory("Appearance"),
    DefaultValue(""),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
    ]
    public virtual string Scheme
    {
        get
        {
            string s = Attributes["scheme"];
            return ((s != null) ? s : String.Empty);
        }
        set
        {
            Attributes["scheme"] = MapStringAttributeToString(value);
        }
    }

    protected internal override void Render(HtmlTextWriter writer)
    {
        if (EnableLegacyRendering)
        {
            base.Render(writer);
        }
        else
        {
            // By default HTMLControl doesn't render a closing /> so its not XHTML compliance
            writer.WriteBeginTag(TagName);
            RenderAttributes(writer);
            writer.Write(HtmlTextWriter.SelfClosingTagEnd);
        }
    }
}
