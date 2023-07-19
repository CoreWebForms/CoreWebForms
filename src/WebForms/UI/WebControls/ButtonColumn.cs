//MIT license

using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace System.Web.UI.WebControls;

/// <devdoc>
/// <para>Creates a column with a set of <see cref='System.Web.UI.WebControls.Button'/>
/// controls.</para>
/// </devdoc>
public class ButtonColumn : DataGridColumn {

    private PropertyDescriptor textFieldDesc;

    /// <devdoc>
    /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.ButtonColumn'/> class.</para>
    /// </devdoc>
    public ButtonColumn() {
    }

    /// <devdoc>
    ///    <para>Gets or sets the type of button to render in the
    ///       column.</para>
    /// </devdoc>
    [
    WebCategory("Appearance"),
    DefaultValue(ButtonColumnType.LinkButton),
    WebSysDescriptionAttribute(SR.ButtonColumn_ButtonType)
    ]
    public virtual ButtonColumnType ButtonType {
        get {
            var o = ViewState["ButtonType"];
            return o != null ? (ButtonColumnType)o : ButtonColumnType.LinkButton;
        }
        set {
            if (value < ButtonColumnType.LinkButton || value > ButtonColumnType.PushButton) {
                throw new ArgumentOutOfRangeException(nameof(value));
            }
            ViewState["ButtonType"] = value;
            OnColumnChanged();
        }
    }

    [
    DefaultValue(false),
    WebSysDescriptionAttribute(SR.ButtonColumn_CausesValidation)
    ]
    public virtual bool CausesValidation {
        get {
            var o = ViewState["CausesValidation"];
            return o != null ? (bool)o : false;
        }
        set {
            ViewState["CausesValidation"] = value;
            OnColumnChanged();
        }
    }

    /// <devdoc>
    /// <para>Gets or sets the command to perform when this <see cref='System.Web.UI.WebControls.Button'/>
    /// is clicked.</para>
    /// </devdoc>
    [
    WebCategory("Behavior"),
    DefaultValue(""),
    WebSysDescriptionAttribute(SR.WebControl_CommandName)
    ]
    public virtual string CommandName {
        get {
            var o = ViewState["CommandName"];
            return o != null ? (string)o : string.Empty;
        }
        set {
            ViewState["CommandName"] = value;
            OnColumnChanged();
        }
    }

    /// <devdoc>
    ///    <para>Gets or sets the field name from the data model that is
    ///       bound to the <see cref='System.Web.UI.WebControls.ButtonColumn.Text'/> property of the button in this column.</para>
    /// </devdoc>
    [
    WebCategory("Data"),
    DefaultValue(""),
    WebSysDescriptionAttribute(SR.ButtonColumn_DataTextField)
    ]
    public virtual string DataTextField {
        get {
            var o = ViewState["DataTextField"];
            return o != null ? (string)o : string.Empty;
        }
        set {
            ViewState["DataTextField"] = value;
            OnColumnChanged();
        }
    }

    /// <devdoc>
    ///    <para>Gets or sets the string used to format the data bound to
    ///       the <see cref='System.Web.UI.WebControls.ButtonColumn.Text'/> property of the button.</para>
    /// </devdoc>
    [
    WebCategory("Data"),
    DefaultValue(""),
    WebSysDescriptionAttribute(SR.ButtonColumn_DataTextFormatString)
    ]
    public virtual string DataTextFormatString {
        get {
            var o = ViewState["DataTextFormatString"];
            return o != null ? (string)o : string.Empty;
        }
        set {
            ViewState["DataTextFormatString"] = value;
            OnColumnChanged();
        }
    }

    /// <devdoc>
    /// <para>Gets or sets the caption text displayed on the <see cref='System.Web.UI.WebControls.Button'/>
    /// in this column.</para>
    /// </devdoc>
    [
    Localizable(true),
    WebCategory("Appearance"),
    DefaultValue(""),
    WebSysDescriptionAttribute(SR.ButtonColumn_Text)
    ]
    public virtual string Text {
        get {
            var o = ViewState["Text"];
            return o != null ? (string)o : string.Empty;
        }
        set {
            ViewState["Text"] = value;
            OnColumnChanged();
        }
    }

    [
    DefaultValue(""),
    WebSysDescriptionAttribute(SR.ButtonColumn_ValidationGroup)
    ]
    public virtual string ValidationGroup {
        get {
            var o = ViewState["ValidationGroup"];
            return o != null ? (string)o : string.Empty;
        }
        set {
            ViewState["ValidationGroup"] = value;
            OnColumnChanged();
        }
    }

    /// <devdoc>
    /// </devdoc>
    protected virtual string FormatDataTextValue(object dataTextValue) {
        var formattedTextValue = string.Empty;

        if (!DataBinder.IsNull(dataTextValue)) {
            var formatting = DataTextFormatString;
            if (formatting.Length == 0) {
                formattedTextValue = dataTextValue.ToString();
            }
            else {
                formattedTextValue = string.Format(CultureInfo.CurrentCulture, formatting, dataTextValue);
            }
        }

        return formattedTextValue;
    }

    /// <devdoc>
    /// </devdoc>
    public override void Initialize() {
        base.Initialize();
        textFieldDesc = null;
    }

    /// <devdoc>
    /// <para>Initializes a cell in the <see cref='System.Web.UI.WebControls.ButtonColumn'/> .</para>
    /// </devdoc>
    public override void InitializeCell(TableCell cell, int columnIndex, ListItemType itemType) {
        base.InitializeCell(cell, columnIndex, itemType);

        if ((itemType != ListItemType.Header) &&
            (itemType != ListItemType.Footer)) {
            WebControl buttonControl = null;

            if (ButtonType == ButtonColumnType.LinkButton) {
                LinkButton button = new DataGridLinkButton();

                button.Text = Text;
                button.CommandName = CommandName;
                button.CausesValidation = CausesValidation;
                button.ValidationGroup = ValidationGroup;
                buttonControl = button;
            }
            else {
                var button = new Button();

                button.Text = Text;
                button.CommandName = CommandName;
                button.CausesValidation = CausesValidation;
                button.ValidationGroup = ValidationGroup;
                buttonControl = button;
            }

            if (DataTextField.Length != 0) {
                buttonControl.DataBinding += new EventHandler(this.OnDataBindColumn);
            }

            cell.Controls.Add(buttonControl);
        }
    }

    /// <devdoc>
    /// </devdoc>
    private void OnDataBindColumn(object sender, EventArgs e) {
        Debug.Assert(DataTextField.Length != 0, "Shouldn't be DataBinding without a DataTextField");

        var boundControl = (Control)sender;
        var item = (DataGridItem)boundControl.NamingContainer;
        var dataItem = item.DataItem;

        if (textFieldDesc == null) {
            var dataField = DataTextField;

            textFieldDesc = TypeDescriptor.GetProperties(dataItem).Find(dataField, true);
            if ((textFieldDesc == null) && !DesignMode) {
                throw new HttpException(SR.GetString(SR.Field_Not_Found, dataField));
            }
        }

        string dataValue;

        if (textFieldDesc != null) {
            var data = textFieldDesc.GetValue(dataItem);
            dataValue = FormatDataTextValue(data);
        }
        else {
            Debug.Assert(DesignMode == true);
            dataValue = SR.GetString(SR.Sample_Databound_Text);
        }

        if (boundControl is LinkButton) {
            ((LinkButton)boundControl).Text = dataValue;
        }
        else {
            Debug.Assert(boundControl is Button, "Expected the bound control to be a Button");
            ((Button)boundControl).Text = dataValue;
        }
    }
}

