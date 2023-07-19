//MIT licensed

using System.ComponentModel;

namespace System.Web.UI.WebControls;

/// <devdoc>
/// <para>Creates a special column with buttons for <see langword='Edit'/>,
/// <see langword='Update'/>, and <see langword='Cancel'/> commands to edit items
///    within the selected row.</para>
/// </devdoc>
public class EditCommandColumn : DataGridColumn
{
    /// <devdoc>
    /// <para>Initializes a new instance of an <see cref='System.Web.UI.WebControls.EditCommandColumn'/> class.</para>
    /// </devdoc>
    public EditCommandColumn()
    {
    }

    /// <devdoc>
    ///    <para>Indicates the button type for the column.</para>
    /// </devdoc>
    [
    DefaultValue(ButtonColumnType.LinkButton)
    ]
    public virtual ButtonColumnType ButtonType
    {
        get
        {
            object o = ViewState["ButtonType"];
            return o != null ? (ButtonColumnType)o : ButtonColumnType.LinkButton;
        }
        set
        {
            if (value < ButtonColumnType.LinkButton || value > ButtonColumnType.PushButton)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }
            ViewState["ButtonType"] = value;
            OnColumnChanged();
        }
    }

    /// <devdoc>
    /// <para>Indicates the text to display for the <see langword='Cancel'/> command button
    ///    in the column.</para>
    /// </devdoc>
    [
    Localizable(true),
    DefaultValue("")
    ]
    public virtual string CancelText
    {
        get
        {
            object o = ViewState["CancelText"];
            return o != null ? (string)o : string.Empty;
        }
        set
        {
            ViewState["CancelText"] = value;
            OnColumnChanged();
        }
    }

    [
    DefaultValue(true),
    ]
    public virtual bool CausesValidation
    {
        get
        {
            object o = ViewState["CausesValidation"];
            return o != null ? (bool)o : true;
        }
        set
        {
            ViewState["CausesValidation"] = value;
            OnColumnChanged();
        }
    }

    /// <devdoc>
    /// <para>Indicates the text to display for the <see langword='Edit'/> command button in
    ///    the column.</para>
    /// </devdoc>
    [
    Localizable(true),
    DefaultValue("")
    ]
    public virtual string EditText
    {
        get
        {
            object o = ViewState["EditText"];
            return o != null ? (string)o : string.Empty;
        }
        set
        {
            ViewState["EditText"] = value;
            OnColumnChanged();
        }
    }

    /// <devdoc>
    /// <para>Indicates the text to display for the <see langword='Update'/> command button
    ///    in the column.</para>
    /// </devdoc>
    [
    Localizable(true),
    DefaultValue("")
    ]
    public virtual string UpdateText
    {
        get
        {
            object o = ViewState["UpdateText"];
            return o != null ? (string)o : string.Empty;
        }
        set
        {
            ViewState["UpdateText"] = value;
            OnColumnChanged();
        }
    }

    [
    DefaultValue(""),
    ]
    public virtual string ValidationGroup
    {
        get
        {
            object o = ViewState["ValidationGroup"];
            return o != null ? (string)o : string.Empty;
        }
        set
        {
            ViewState["ValidationGroup"] = value;
            OnColumnChanged();
        }
    }

    private void AddButtonToCell(TableCell cell, string commandName, string buttonText, bool causesValidation, string validationGroup)
    {
        ControlCollection controls = cell.Controls;
        ButtonColumnType buttonType = ButtonType;

        WebControl buttonControl;
        if (buttonType == ButtonColumnType.LinkButton)
        {
            LinkButton button = new DataGridLinkButton();

            buttonControl = button;
            button.CommandName = commandName;
            button.Text = buttonText;
            button.CausesValidation = causesValidation;
            button.ValidationGroup = validationGroup;
        }
        else
        {
            Button button = new Button();

            buttonControl = button;
            button.CommandName = commandName;
            button.Text = buttonText;
            button.CausesValidation = causesValidation;
            button.ValidationGroup = validationGroup;
        }

        controls.Add(buttonControl);
    }

    /// <devdoc>
    ///    <para>Initializes a cell within the column.</para>
    /// </devdoc>
    public override void InitializeCell(TableCell cell, int columnIndex, ListItemType itemType)
    {
        base.InitializeCell(cell, columnIndex, itemType);
        bool causesValidation = CausesValidation;

        if ((itemType != ListItemType.Header) &&
            (itemType != ListItemType.Footer))
        {
            if (itemType == ListItemType.EditItem)
            {
                ControlCollection controls = cell.Controls;
                AddButtonToCell(cell, DataGrid.UpdateCommandName, UpdateText, causesValidation, ValidationGroup);

                LiteralControl spaceControl = new LiteralControl("&nbsp;");
                controls.Add(spaceControl);

                AddButtonToCell(cell, DataGrid.CancelCommandName, CancelText, false, string.Empty);
            }
            else
            {
                AddButtonToCell(cell, DataGrid.EditCommandName, EditText, false, string.Empty);
            }
        }
    }
}

