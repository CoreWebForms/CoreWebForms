// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using System.Globalization;

#nullable disable

namespace System.Web.UI.WebControls;
// The reason we define this empty override in the WebControls namespace is
// to expose it as a control that can be used on a page (ASURT 54683)
// E.g. <asp:literal runat=server id=lit1/>

[
DataBindingHandler("System.Web.UI.Design.TextDataBindingHandler, " + AssemblyRef.SystemDesign),
DefaultProperty("Text"),
Designer("System.Web.UI.Design.WebControls.LiteralDesigner, " + AssemblyRef.SystemDesign),
]
public class Literal : Control, ITextControl
{

    /// <devdoc>
    ///     [To be supplied.]
    /// </devdoc>
    [
    DefaultValue(LiteralMode.Transform),
    WebCategory("Behavior"),
    WebSysDescription(SR.Literal_Mode)
    ]
    public LiteralMode Mode
    {
        get
        {
            object mode = ViewState["Mode"];
            return mode == null ? LiteralMode.Transform : (LiteralMode)mode;
        }
        set
        {
            if ((value < LiteralMode.Transform) || (value > LiteralMode.Encode))
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            ViewState["Mode"] = value;
        }
    }

    /// <devdoc>
    ///     [To be supplied.]
    /// </devdoc>
    [
    Localizable(true),
    Bindable(true),
    WebCategory("Appearance"),
    DefaultValue(""),
    WebSysDescription(SR.Literal_Text),
    ]
    public string Text
    {
        get
        {
            string s = (string)ViewState["Text"];
            return s ?? string.Empty;
        }
        set
        {
            ViewState["Text"] = value;
        }
    }

    /// <internalonly/>
    /// <devdoc>
    ///     [To be supplied.]
    /// </devdoc>
    protected override void AddParsedSubObject(object obj)
    {
        if (obj is LiteralControl)
        {
            Text = ((LiteralControl)obj).Text;
        }
        else
        {
            throw new HttpException(SR.GetString(SR.Cannot_Have_Children_Of_Type, "Literal", obj.GetType().Name.ToString(CultureInfo.InvariantCulture)));
        }
    }

    /// <devdoc>
    ///     [To be supplied.]
    /// </devdoc>
    protected override ControlCollection CreateControlCollection()
    {
        return new EmptyControlCollection(this);
    }

    /// <devdoc>
    /// </devdoc>
    [
    EditorBrowsable(EditorBrowsableState.Never),
    ]
    public override void Focus()
    {
        throw new NotSupportedException(SR.GetString(SR.NoFocusSupport, this.GetType().Name));
    }

    /// <devdoc>
    ///     [To be supplied.]
    /// </devdoc>
    protected internal override void Render(HtmlTextWriter writer)
    {
        string text = Text;
        if (text.Length != 0)
        {
            if (Mode != LiteralMode.Encode)
            {
                writer.Write(text);
                return;
            }

            HttpUtility.HtmlEncode(text, writer);
        }
    }
}
