// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web.UI.WebControls;

using System;
using System.Collections;
using System.Drawing;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.Features;

public class WebControl : Control
{
    private static string _disabledCssClass = "aspNetDisabled";

    private readonly HtmlTextWriterTag _tagKey;

    private string? _tagName;
    private AttributeCollection? _attributeCollection;
    private StateBag? _attributeState;
    private Style? _controlStyle;

    protected WebControl()
        : this(HtmlTextWriterTag.Span)
    {
    }

    public WebControl(HtmlTextWriterTag tag)
    {
        _tagKey = tag;
    }

    protected WebControl(string tag)
    {
        _tagKey = HtmlTextWriterTag.Unknown;
        _tagName = tag;
    }

    public virtual string AccessKey
    {
        get => ViewState["AccessKey"] as string ?? string.Empty;
        set
        {
            // Valid values are null, String.Empty, and single character strings
            if ((value != null) && (value.Length > 1))
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Invalid AccessKey value");
            }

            ViewState["AccessKey"] = value;
        }
    }

    public AttributeCollection Attributes
    {
        get
        {
            if (_attributeCollection == null)
            {
                if (_attributeState == null)
                {
                    _attributeState = new StateBag(true);
                    if (IsTrackingViewState)
                    {
                        _attributeState.TrackViewState();
                    }
                }

                _attributeCollection = new AttributeCollection(_attributeState);
            }

            return _attributeCollection;
        }
    }

    public virtual Color BackColor
    {
        get => ControlStyleCreated == false ? Color.Empty : ControlStyle.BackColor;
        set => ControlStyle.BackColor = value;
    }

    public virtual Color BorderColor
    {
        get => ControlStyleCreated == false ? Color.Empty : ControlStyle.BorderColor;
        set => ControlStyle.BorderColor = value;
    }

    public virtual Unit BorderWidth
    {
        get => ControlStyleCreated == false ? Unit.Empty : ControlStyle.BorderWidth;
        set => ControlStyle.BorderWidth = value;
    }

    public virtual BorderStyle BorderStyle
    {
        get => ControlStyleCreated == false ? BorderStyle.NotSet : ControlStyle.BorderStyle;
        set => ControlStyle.BorderStyle = value;
    }

    public Style ControlStyle
    {
        get
        {
            if (_controlStyle == null)
            {
                _controlStyle = CreateControlStyle();
                if (IsTrackingViewState)
                {
                    _controlStyle.TrackViewState();
                }
            }
            return _controlStyle;
        }
    }

    public bool ControlStyleCreated => _controlStyle != null;

    public virtual string CssClass
    {
        get => ControlStyleCreated == false ? string.Empty : ControlStyle.CssClass;
        set => ControlStyle.CssClass = value;
    }

    public static string DisabledCssClass
    {
        get => _disabledCssClass ?? string.Empty;
        set => _disabledCssClass = value;
    }

    public CssStyleCollection Style => Attributes.CssStyle;

    public virtual bool Enabled { get; set; }

    public virtual FontInfo Font => ControlStyle.Font;

    public virtual Color ForeColor
    {
        get => ControlStyleCreated == false ? Color.Empty : ControlStyle.ForeColor;
        set => ControlStyle.ForeColor = value;
    }

    public bool HasAttributes => ((_attributeCollection != null) && (_attributeCollection.Count > 0)) || ((_attributeState != null) && (_attributeState.Count > 0));

    public virtual Unit Height
    {
        get => ControlStyleCreated == false ? Unit.Empty : ControlStyle.Height;
        set => ControlStyle.Height = value;
    }

    protected internal bool IsEnabled => GetHierarchicalFeature<IsEnabledFeature>()?.IsEnabled ?? true;

    public virtual bool SupportsDisabledAttribute => true;

    internal virtual bool RequiresLegacyRendering => false;

    public virtual short TabIndex
    {
        get => ViewState["TabIndex"] is short o ? o : (short)0;
        set => ViewState["TabIndex"] = value;
    }

    protected virtual HtmlTextWriterTag TagKey => _tagKey;

    protected virtual string? TagName
    {
        get
        {
            if (_tagName == null && TagKey != HtmlTextWriterTag.Unknown)
            {
                // perf: this enum.format wasn't changed to a switch because the TagKey is normally used, not the TagName.
                _tagName = Enum.Format(typeof(HtmlTextWriterTag), TagKey, "G").ToLower(CultureInfo.InvariantCulture);
            }

            return _tagName;
        }
    }

    public virtual string ToolTip
    {
        get => ViewState["ToolTip"] is string toolTip ? toolTip : string.Empty;
        set => ViewState["ToolTip"] = value;
    }

    public virtual Unit Width
    {
        get => ControlStyleCreated == false ? Unit.Empty : ControlStyle.Width;
        set => ControlStyle.Width = value;
    }

    protected virtual void AddAttributesToRender(HtmlTextWriter writer)
    {
        if (ID != null)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Id, ClientID);
        }

        if (AccessKey is { Length: > 0 } s)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Accesskey, s);
        }

        if (!Enabled)
        {
            if (SupportsDisabledAttribute)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");
            }
            if (!string.IsNullOrEmpty(DisabledCssClass))
            {
                if (string.IsNullOrEmpty(CssClass))
                {
                    ControlStyle.CssClass = DisabledCssClass;
                }
                else
                {
                    ControlStyle.CssClass = DisabledCssClass + " " + CssClass;
                }
            }
        }

        if (TabIndex is { } n && n > 0)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Tabindex, n.ToString(NumberFormatInfo.InvariantInfo));
        }

        if (ToolTip is { Length: > 0 } tooltip)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Title, tooltip);
        }

        // VSWhidbey 496445: Setting the specific style display:inline-block common to <span> and <a> tag
        if (TagKey == HtmlTextWriterTag.Span || TagKey == HtmlTextWriterTag.A)
        {
            AddDisplayInlineBlockIfNeeded(writer);
        }

        if (ControlStyleCreated && !ControlStyle.IsEmpty)
        {
            // let the style add attributes
            ControlStyle.AddAttributesToRender(writer, this);
        }

        // add expando attributes
        if (_attributeState != null)
        {
            AttributeCollection atrColl = Attributes;
            IEnumerator keys = atrColl.Keys.GetEnumerator();
            while (keys.MoveNext())
            {
                string attrName = (string)keys.Current;
                writer.AddAttribute(attrName, atrColl[attrName]);
            }
        }
    }

    internal void AddDisplayInlineBlockIfNeeded(HtmlTextWriter writer)
    {
        if (BorderStyle != BorderStyle.NotSet || !BorderWidth.IsEmpty || !Height.IsEmpty || !Width.IsEmpty)
        {
            writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "inline-block");
        }
    }

    public void ApplyStyle(Style s)
    {
        if ((s != null) && (s.IsEmpty == false))
        {
            ControlStyle.CopyFrom(s);
        }
    }

    public void CopyBaseAttributes(WebControl controlSrc)
    {
        if (controlSrc == null)
        {
            throw new ArgumentNullException(nameof(controlSrc));
        }

        AccessKey = controlSrc.AccessKey;
        Enabled = false;
        ToolTip = controlSrc.ToolTip;
        TabIndex = controlSrc.TabIndex;

        if (controlSrc.HasAttributes)
        {
            foreach (string key in controlSrc.Attributes.Keys)
            {
                Attributes[key] = controlSrc.Attributes[key];
            }
        }
    }

    protected virtual Style CreateControlStyle()
    {
        return new Style(ViewState);
    }

    public void MergeStyle(Style s)
    {
        if ((s != null) && (s.IsEmpty == false))
        {
            ControlStyle.MergeWith(s);
        }
    }

    protected internal override void Render(HtmlTextWriter writer)
    {
        RenderBeginTag(writer);
        RenderContents(writer);
        RenderEndTag(writer);
    }

    public virtual void RenderBeginTag(HtmlTextWriter writer)
    {
        AddAttributesToRender(writer);

        HtmlTextWriterTag tagKey = TagKey;
        if (tagKey != HtmlTextWriterTag.Unknown)
        {
            writer.RenderBeginTag(tagKey);
        }
        else
        {
            writer.RenderBeginTag(TagName);
        }
    }

    public virtual void RenderEndTag(HtmlTextWriter writer)
    {
        writer.RenderEndTag();
    }

    protected internal virtual void RenderContents(HtmlTextWriter writer)
    {
        base.Render(writer);
    }
}

