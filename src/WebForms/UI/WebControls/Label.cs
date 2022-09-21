// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

namespace System.Web.UI.WebControls;
public class Label : WebControl, ITextControl
{
    private bool _textSetByAddParsedSubObject;

    /// <devdoc>
    /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.Label'/> class and renders
    ///    it as a SPAN tag.</para>
    /// </devdoc>
    public Label()
    {
    }

    /// <devdoc>
    /// </devdoc>
    internal Label(HtmlTextWriterTag tag) : base(tag)
    {
    }

    public virtual string AssociatedControlID
    {
        get
        {
            string s = (string)ViewState["AssociatedControlID"];
            return s ?? string.Empty;
        }
        set
        {
            ViewState["AssociatedControlID"] = value;
        }
    }

    internal bool AssociatedControlInControlTree
    {
        get
        {
            object o = ViewState["AssociatedControlNotInControlTree"];
            return o == null ? true : (bool)o;
        }
        set
        {
            ViewState["AssociatedControlNotInControlTree"] = value;
        }
    }

    public override bool SupportsDisabledAttribute =>
            //return RenderingCompatibility < VersionUtil.Framework40;
            true;

    internal override bool RequiresLegacyRendering => true;

    protected override HtmlTextWriterTag TagKey => AssociatedControlID.Length != 0 ? HtmlTextWriterTag.Label : base.TagKey;

    public virtual string Text
    {
        get
        {
            object o = ViewState["Text"];
            return (o == null) ? string.Empty : (string)o;
        }
        set
        {
            if (HasControls())
            {
                Controls.Clear();
            }
            ViewState["Text"] = value;
        }
    }

    protected override void AddAttributesToRender(HtmlTextWriter writer)
    {
        string associatedControlID = AssociatedControlID;
        if (associatedControlID.Length != 0)
        {
            if (AssociatedControlInControlTree)
            {
                Control wc = FindControl(associatedControlID);
                if (wc == null)
                {
                    // Don't throw in the designer.
                    if (!DesignMode)
                    {
                        throw new HttpException(SR.GetString(SR.LabelForNotFound, associatedControlID, ID));
                    }
                }
                else
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.For, wc.ClientID);
                }
            }
            else
            {
                writer.AddAttribute(HtmlTextWriterAttribute.For, associatedControlID);
            }

        }

        base.AddAttributesToRender(writer);
    }

    protected override void AddParsedSubObject(object obj)
    {
        if (HasControls())
        {
            base.AddParsedSubObject(obj);
        }
        else
        {
            if (obj is LiteralControl)
            {
                if (_textSetByAddParsedSubObject)
                {
                    Text += ((LiteralControl)obj).Text;
                }
                else
                {
                    Text = ((LiteralControl)obj).Text;
                }
                _textSetByAddParsedSubObject = true;
            }
            else
            {
                string currentText = Text;
                if (currentText.Length != 0)
                {
                    Text = string.Empty;
                    base.AddParsedSubObject(new LiteralControl(currentText));
                }
                base.AddParsedSubObject(obj);
            }
        }
    }

    protected override void LoadViewState(object savedState)
    {
        if (savedState != null)
        {
            base.LoadViewState(savedState);

            string s = (string)ViewState["Text"];
            // Dev10 703061 If Text is set, we want to clear out any child controls, but not dirty viewstate
            if (s != null && HasControls())
            {
                Controls.Clear();
            }
        }
    }

    protected internal override void RenderContents(HtmlTextWriter writer)
    {
        if (HasRenderingData())
        {
            base.RenderContents(writer);
        }
        else
        {
            writer.Write(Text);
        }
    }
}
