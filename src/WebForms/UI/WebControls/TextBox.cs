// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web.UI.WebControls;

using System;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Drawing.Design;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.Util;
using Microsoft.Extensions.Logging;

public class TextBox : WebControl, IPostBackDataHandler, IEditableTextControl
{
    private static readonly object EventTextChanged = new object();

    private const string _textBoxKeyHandlerCall = "if (WebForm_TextBoxKeyHandler(event) == false) return false;";

    private const int DefaultMutliLineRows = 2;
    private const int DefaultMutliLineColumns = 20;

    /// <devdoc>
    /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.TextBox'/> class.</para>
    /// </devdoc>
    public TextBox() : base(HtmlTextWriterTag.Input)
    {
    }

    public virtual AutoCompleteType AutoCompleteType
    {
        get
        {
            object obj = ViewState["AutoCompleteType"];
            return (obj == null) ? AutoCompleteType.None : (AutoCompleteType)obj;
        }
        set
        {
            if (value < AutoCompleteType.None || value > AutoCompleteType.Enabled)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }
            ViewState["AutoCompleteType"] = value;
        }
    }

    public virtual bool AutoPostBack
    {
        get
        {
            object b = ViewState["AutoPostBack"];
            return ((b == null) ? false : (bool)b);
        }
        set
        {
            ViewState["AutoPostBack"] = value;
        }
    } 

    public virtual bool CausesValidation
    {
        get
        {
            object b = ViewState["CausesValidation"];
            return ((b == null) ? false : (bool)b);
        }
        set
        {
            ViewState["CausesValidation"] = value;
        }
    }

    public virtual int Columns
    {
        get
        {
            object o = ViewState["Columns"];
            return ((o == null) ? 0 : (int)o);
        }
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException("Columns", "Invalid columns");
            }

            ViewState["Columns"] = value;
        }
    }

    public virtual int MaxLength
    {
        get
        {
            object o = ViewState["MaxLength"];
            return ((o == null) ? 0 : (int)o);
        }
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }
            ViewState["MaxLength"] = value;
        }
    }

    public virtual TextBoxMode TextMode
    {
        get
        {
            object mode = ViewState["Mode"];
            return ((mode == null) ? TextBoxMode.SingleLine : (TextBoxMode)mode);
        }
        set
        {
            if (value < TextBoxMode.SingleLine || value > TextBoxMode.Week)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }
            ViewState["Mode"] = value;
        }
    }

    public virtual bool ReadOnly
    {
        get
        {
            object o = ViewState["ReadOnly"];
            return ((o == null) ? false : (bool)o);
        }
        set
        {
            ViewState["ReadOnly"] = value;
        }
    }

    public virtual int Rows
    {
        get
        {
            object o = ViewState["Rows"];
            return ((o == null) ? 0 : (int)o);
        }
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException("Rows", "Invalid text rows");
            }
            ViewState["Rows"] = value;
        }
    }

    private bool SaveTextViewState
    {
        get
        {
            // 

            // Must be saved when
            // 1. There is a registered event handler for SelectedIndexChanged
            // 2. Control is not enabled or visible, because the browser's post data will not include this control
            // 3. The instance is a derived instance, which might be overriding the OnTextChanged method

            if (TextMode == TextBoxMode.Password)
            {
                return false;
            }

            if ((Events[EventTextChanged] != null) ||
                (IsEnabled == false) ||
                (Visible == false) ||
                (ReadOnly) ||
                (this.GetType() != typeof(TextBox)))
            {
                return true;
            }

            return false;
        }
    }

    protected override HtmlTextWriterTag TagKey
        => TextMode == TextBoxMode.MultiLine ? HtmlTextWriterTag.Textarea : HtmlTextWriterTag.Input;

    [AllowNull]
    public virtual string Text
    {
        get
        {
            string s = (string)ViewState["Text"];
            return ((s == null) ? string.Empty : s);
        }
        set
        {
            ViewState["Text"] = value;
        }
    }

    public virtual string ValidationGroup
    {
        get
        {
            string s = (string)ViewState["ValidationGroup"];
            return ((s == null) ? string.Empty : s);
        }
        set
        {
            ViewState["ValidationGroup"] = value;
        }
    }

    public virtual bool Wrap
    {
        get
        {
            object b = ViewState["Wrap"];
            return ((b == null) ? true : (bool)b);
        }
        set
        {
            ViewState["Wrap"] = value;
        }
    }

    public event EventHandler TextChanged
    {
        add
        {
            Events.AddHandler(EventTextChanged, value);
        }
        remove
        {
            Events.RemoveHandler(EventTextChanged, value);
        }
    }

    /// <internalonly/>
    /// <devdoc>
    /// </devdoc>
    protected override void AddAttributesToRender(HtmlTextWriter writer)
    {
        // Make sure we are in a form tag with runat=server.
        var page = Page;
        if (page != null)
        {
            // TODO
            //page.VerifyRenderingInServerForm(this);
        }

        var uniqueID = UniqueID;
        if (uniqueID != null)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Name, uniqueID);
        }

        TextBoxMode mode = TextMode;

        if (mode == TextBoxMode.MultiLine)
        {
            // MultiLine renders as textarea

            int rows = Rows;
            int columns = Columns;
            bool adapterRenderZeroRowCol = false;

            // VSWhidbey 497755
            if (rows == 0)
            {
                rows = DefaultMutliLineRows;
            }
            if (columns == 0)
            {
                columns = DefaultMutliLineColumns;
            }

            if (rows > 0 || adapterRenderZeroRowCol)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Rows, rows.ToString(NumberFormatInfo.InvariantInfo));
            }

            if (columns > 0 || adapterRenderZeroRowCol)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Cols, columns.ToString(NumberFormatInfo.InvariantInfo));
            }

            if (!Wrap)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Wrap, "off");
            }

            //VSO449020 Add MaxLength Support for mutiple lines textbox, since in HTML5 this attribute is supported for textarea.
            if (MaxLength > 0)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Maxlength, MaxLength.ToString(NumberFormatInfo.InvariantInfo));
            }
        }
        else
        {
            // Everything else renders as input
            if (mode != TextBoxMode.SingleLine || string.IsNullOrEmpty(Attributes["type"]))
            {
                // If the developer specified a custom type (like an HTML 5 type), use that type instead of "text".
                // The call to base.AddAttributesToRender at the end of this method will add the custom type if specified.
                writer.AddAttribute(HtmlTextWriterAttribute.Type, GetTypeAttributeValue(mode));
            }

            AutoCompleteType autoCompleteType = AutoCompleteType;
            if (mode == TextBoxMode.SingleLine &&
                autoCompleteType != AutoCompleteType.None &&
                autoCompleteType != AutoCompleteType.Enabled &&
                autoCompleteType != AutoCompleteType.Disabled)
            {

                // Renders the vcard_name attribute so that client browsers can support autocomplete
                string name = GetVCardAttributeValue(autoCompleteType);
                writer.AddAttribute(HtmlTextWriterAttribute.VCardName, name);
            }

            if (autoCompleteType == AutoCompleteType.Disabled &&
                (mode >= TextBoxMode.Color || (mode == TextBoxMode.SingleLine)))
            {
                // Only render autocomplete="off" when one of the following is true
                // - 4.5 or higher rendering compat is being used
                // - any of the new HTML5 modes are being used
                // - browser supports vCard AND mode is SingleLine (this is the legacy pre-4.5 behavior)
                writer.AddAttribute(HtmlTextWriterAttribute.AutoComplete, "off");
            }

            if (autoCompleteType == AutoCompleteType.Enabled)
            {
                // Since Enabled is a new value in .NET 4.5 we don't need back-compat switches
                writer.AddAttribute(HtmlTextWriterAttribute.AutoComplete, "on");
            }

            if (mode != TextBoxMode.Password)
            {
                // only render value if we're not a password
                string s = Text;
                if (s.Length > 0)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Value, s);
                }
            }

            int n = MaxLength;
            if (n > 0)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Maxlength, n.ToString(NumberFormatInfo.InvariantInfo));
            }
            n = Columns;
            if (n > 0)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Size, n.ToString(NumberFormatInfo.InvariantInfo));
            }
        }

        if (ReadOnly)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.ReadOnly, "readonly");
        }

        if (AutoPostBack && (page != null))
        {
            string? onChange = null;
            if (HasAttributes)
            {
                onChange = Attributes["onchange"];
                if (onChange != null)
                {
                    onChange = Util.EnsureEndWithSemiColon(onChange);
                    Attributes.Remove("onchange");
                }
            }

            PostBackOptions options = new PostBackOptions(this, string.Empty);

            // ASURT 98368
            // Need to merge the autopostback script with the user script
            if (CausesValidation)
            {
                options.PerformValidation = true;
                options.ValidationGroup = ValidationGroup;
            }

            if (page.Form != null)
            {
                options.AutoPostBack = true;
            }

            // TODO: Enable scripts
            //onChange = Util.MergeScript(onChange, page.ClientScript.GetPostBackEventReference(options, true));
            writer.AddAttribute(HtmlTextWriterAttribute.Onchange, onChange);

            if (mode != TextBoxMode.MultiLine)
            {
                string onKeyPress = _textBoxKeyHandlerCall;
                if (HasAttributes)
                {
                    var userOnKeyPress = Attributes["onkeypress"];
                    if (userOnKeyPress != null)
                    {
                        onKeyPress += userOnKeyPress;
                        Attributes.Remove("onkeypress");
                    }
                }
                writer.AddAttribute("onkeypress", onKeyPress);
            }
        }
        else if (page != null)
        {
            // TODO: Enable scripts
            //page.ClientScript.RegisterForEventValidation(this.UniqueID, string.Empty);
        }

        if (Enabled && !IsEnabled && SupportsDisabledAttribute)
        {
            // We need to do the cascade effect on the server, because the browser
            // only renders as disabled, but doesn't disable the functionality.
            writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");
        }

        base.AddAttributesToRender(writer);
    }

    internal static string GetTypeAttributeValue(TextBoxMode mode)
    {
        switch (mode)
        {
            case TextBoxMode.SingleLine: return "text";
            case TextBoxMode.Password: return "password";
            case TextBoxMode.Color: return "color";
            case TextBoxMode.Date: return "date";
            case TextBoxMode.DateTime: return "datetime";
            case TextBoxMode.DateTimeLocal: return "datetime-local";
            case TextBoxMode.Email: return "email";
            case TextBoxMode.Month: return "month";
            case TextBoxMode.Number: return "number";
            case TextBoxMode.Range: return "range";
            case TextBoxMode.Search: return "search";
            case TextBoxMode.Phone: return "tel";
            case TextBoxMode.Time: return "time";
            case TextBoxMode.Url: return "url";
            case TextBoxMode.Week: return "week";
            case TextBoxMode.MultiLine:
            // falling through on purpose
            default:
                // the default case could only happen if
                //  - someone forces an out-of-range value as a TextBoxMode and passes it in
                //  - a new TextBoxMode value gets added, in which case it should be handled as an explicit case above
                throw new InvalidOperationException();
        }
    }

    internal static string GetVCardAttributeValue(AutoCompleteType type)
    {
        switch (type)
        {
            case AutoCompleteType.None:
            case AutoCompleteType.Disabled:
            case AutoCompleteType.Enabled:
                // should not happen
                throw new InvalidOperationException();
            case AutoCompleteType.Search:
                return "search";
            case AutoCompleteType.HomeCountryRegion:
                return "HomeCountry";
            case AutoCompleteType.BusinessCountryRegion:
                return "BusinessCountry";
            default:
                string result = Enum.Format(typeof(AutoCompleteType), type, "G");
                // Business and Home properties need to be prefixed with "."
                if (result.StartsWith("Business", StringComparison.Ordinal))
                {
                    result = result.Insert(8, ".");
                }
                else if (result.StartsWith("Home", StringComparison.Ordinal))
                {
                    result = result.Insert(4, ".");
                }
                return "vCard." + result;
        }
    }

    /// <internalonly/>
    protected internal override void OnPreRender(EventArgs e)
    {
        base.OnPreRender(e);

        if (Page is { } page && IsEnabled)
        {
            if (SaveTextViewState == false)
            {
                // Store a client-side array of enabled control, so we can re-enable them on
                // postback (in case they are disabled client-side)
                // Postback is needed when view state for the Text property is disabled
                page.RegisterEnabledControl(this);
            }

            if (AutoPostBack)
            {
                page.RegisterWebFormsScript();
                page.RegisterPostBackScript();
                page.RegisterFocusScript();
            }
        }
    }

    /// <internalonly/>
    /// <devdoc>
    /// <para>Loads the posted text box content if it is different
    /// from the last posting.</para>
    /// </devdoc>
    bool IPostBackDataHandler.LoadPostData(string postDataKey, NameValueCollection postCollection)
    {
        return LoadPostData(postDataKey, postCollection);
    }

    /// <internalonly/>
    /// <devdoc>
    /// <para>Loads the posted text box content if it is different
    /// from the last posting.</para>
    /// </devdoc>
    protected virtual bool LoadPostData(string postDataKey, NameValueCollection postCollection)
    {
        ValidateEvent(postDataKey);

        var current = Text;
        var postData = postCollection[postDataKey];

        // VSWhidbey 442850: Everett had current.Equals(postData), and it is
        // equivalent to the option StringComparison.Ordinal in Whidbey
        if (!ReadOnly && !current.Equals(postData, StringComparison.Ordinal))
        {
            Text = postData;
            return true;
        }
        return false;
    }

    /// <devdoc>
    /// <para> Raises the <see langword='TextChanged'/> event.</para>
    /// </devdoc>
    protected virtual void OnTextChanged(EventArgs e)
    {
        if (Events[EventTextChanged] is EventHandler onChangeHandler)
        {
            onChangeHandler(this, e);
        }
    }

    /// <internalonly/>
    /// <devdoc>
    /// <para>Invokes the <see cref='System.Web.UI.WebControls.TextBox.OnTextChanged'/> method
    /// whenever posted data for the text box has changed.</para>
    /// </devdoc>
    void IPostBackDataHandler.RaisePostDataChangedEvent()
    {
        RaisePostDataChangedEvent();
    }

    /// <internalonly/>
    /// <devdoc>
    /// <para>Invokes the <see cref='System.Web.UI.WebControls.TextBox.OnTextChanged'/> method
    /// whenever posted data for the text box has changed.</para>
    /// </devdoc>
    protected virtual void RaisePostDataChangedEvent()
    {
        if (Page is { } page && AutoPostBack && !page.IsPostBackEventControlRegistered)
        {
            page.AutoPostBackControl = this;

            if (CausesValidation)
            {
                page.Validate(ValidationGroup);
            }
        }

        OnTextChanged(EventArgs.Empty);
    }

    /// <internalonly/>
    /// <devdoc>
    /// </devdoc>
    protected internal override void Render(HtmlTextWriter writer)
    {
        RenderBeginTag(writer);
        //Dev10 Bug 483896: Original TextBox rendering in MultiLine mode suffers from the
        //problem of losing the first newline. We fixed this bug by always rendering a newline
        //before rendering the value of the Text property.
        if (TextMode == TextBoxMode.MultiLine)
        {
            writer.Write(System.Environment.NewLine);
            HttpUtility.HtmlEncode(Text, writer);
        }
        RenderEndTag(writer);
    }
}
