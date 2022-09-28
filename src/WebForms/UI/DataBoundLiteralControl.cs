// MIT License.

#nullable disable

using System.ComponentModel;
using System.Text;

namespace System.Web.UI;
/// <devdoc>
/// <para>Defines the properties and methods of the DataBoundLiteralControl class. </para>
/// </devdoc>
[
ToolboxItem(false)
]
public sealed class DataBoundLiteralControl : Control, ITextControl
{
    private readonly string[] _staticLiterals;

    private string[] _dataBoundLiteral;
    private bool _hasDataBoundStrings;

    /// <internalonly/>
    public DataBoundLiteralControl(int staticLiteralsCount, int dataBoundLiteralCount)
    {
        _staticLiterals = new string[staticLiteralsCount];
        _dataBoundLiteral = new string[dataBoundLiteralCount];
        PreventAutoID();
    }

    /// <internalonly/>
    public void SetStaticString(int index, string s)
    {
        _staticLiterals[index] = s;
    }

    /// <internalonly/>
    public void SetDataBoundString(int index, string s)
    {
        _dataBoundLiteral[index] = s;
        _hasDataBoundStrings = true;
    }

    /// <devdoc>
    ///    <para>Gets the text content of the data-bound literal control.</para>
    /// </devdoc>
    public string Text
    {
        get
        {
            StringBuilder sb = new StringBuilder();

            int dataBoundLiteralCount = _dataBoundLiteral.Length;

            // Append literal and databound strings alternatively
            for (int i = 0; i < _staticLiterals.Length; i++)
            {
                if (_staticLiterals[i] != null)
                {
                    sb.Append(_staticLiterals[i]);
                }

                // Could be null if DataBind() was not called
                if (i < dataBoundLiteralCount && _dataBoundLiteral[i] != null)
                {
                    sb.Append(_dataBoundLiteral[i]);
                }
            }

            return sb.ToString();
        }
    }

    /// <internalonly/>
    protected override ControlCollection CreateControlCollection()
    {
        return new EmptyControlCollection(this);
    }

    /// <internalonly/>
    /// <devdoc>
    ///    <para>Loads the previously saved state. Overridden to synchronize Text property with
    ///       LiteralContent.</para>
    /// </devdoc>
    protected override void LoadViewState(object savedState)
    {
        if (savedState != null)
        {
            _dataBoundLiteral = (string[])savedState;
            _hasDataBoundStrings = true;
        }
    }

    /// <internalonly/>
    /// <devdoc>
    ///    <para>The object that contains the state changes. </para>
    /// </devdoc>
    protected override object SaveViewState()
    {

        // Return null if we didn't get any databound strings
        if (!_hasDataBoundStrings)
        {
            return null;
        }

        // Only save the databound literals to the view state
        return _dataBoundLiteral;
    }

    /// <internalonly/>
    protected internal override void Render(HtmlTextWriter output)
    {

        int dataBoundLiteralCount = _dataBoundLiteral.Length;

        // Render literal and databound strings alternatively
        for (int i = 0; i < _staticLiterals.Length; i++)
        {

            if (_staticLiterals[i] != null)
            {
                output.Write(_staticLiterals[i]);
            }

            // Could be null if DataBind() was not called
            if (i < dataBoundLiteralCount && _dataBoundLiteral[i] != null)
            {
                output.Write(_dataBoundLiteral[i]);
            }
        }
    }

    /// <internalonly/>
    /// <devdoc>
    ///    <para>Implementation of TextControl.Text property. </para>
    /// </devdoc>
    string ITextControl.Text
    {
        get
        {
            return Text;
        }
        set
        {
            throw new NotSupportedException();
        }
    }
}

/// <devdoc>
/// <para>Simpler version of DataBoundLiteralControlBuilder, used at design time. </para>
/// </devdoc>
[
DataBindingHandler("System.Web.UI.Design.TextDataBindingHandler, " + AssemblyRef.SystemDesign),
ToolboxItem(false)
]
public sealed class DesignerDataBoundLiteralControl : Control
{
    private string _text;

    public DesignerDataBoundLiteralControl()
    {
        PreventAutoID();
    }

    /// <devdoc>
    ///    <para>Gets or sets the text content of the data-bound literal control.</para>
    /// </devdoc>
    public string Text
    {
        get
        {
            return _text;
        }
        set
        {
            _text = (value != null) ? value : String.Empty;
        }
    }

    protected override ControlCollection CreateControlCollection()
    {
        return new EmptyControlCollection(this);
    }

    /// <devdoc>
    ///    <para>Loads the previously saved state. Overridden to synchronize Text property with
    ///       LiteralContent.</para>
    /// </devdoc>
    protected override void LoadViewState(object savedState)
    {
        if (savedState != null)
        {
            _text = (string)savedState;
        }
    }

    /// <devdoc>
    ///    <para>Saves any state that was modified after the control began monitoring state changes.</para>
    /// </devdoc>
    protected internal override void Render(HtmlTextWriter output)
    {
        output.Write(_text);
    }

    /// <devdoc>
    ///    <para>The object that contains the state changes. </para>
    /// </devdoc>
    protected override object SaveViewState()
    {
        return _text;
    }
}

