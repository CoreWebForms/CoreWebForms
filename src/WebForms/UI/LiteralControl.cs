// MIT License.

#nullable disable

using System.ComponentModel;

namespace System.Web.UI;

/// <devdoc>
/// <para>Defines the properties and methods of the LiteralControl class. A 
///    literal control is usually rendered as HTML text on a page. </para>
/// <para>
///    LiteralControls behave as text holders, i.e., the parent of a LiteralControl may decide
///    to extract its text, and remove the control from its Control collection (typically for
///    performance reasons).
///    Therefore a control derived from LiteralControl must do any preprocessing of its Text
///    when it hands it out, that it would otherwise have done in its Render implementation.
/// </para>
/// </devdoc>
[
ToolboxItem(false)
]
public class LiteralControl : Control, ITextControl
{
    internal string _text;

    /// <devdoc>
    ///    <para>Creates a control that holds a literal string.</para>
    /// </devdoc>
    public LiteralControl()
    {
        PreventAutoID();
        SetEnableViewStateInternal(false);
    }

    /// <devdoc>
    /// <para>Initializes a new instance of the LiteralControl class with
    ///    the specified text.</para>
    /// </devdoc>
    public LiteralControl(string text) : this()
    {
        _text = text ?? string.Empty;
    }

    /// <devdoc>
    ///    <para>Gets or sets the text content of the literal control.</para>
    /// </devdoc>
    public virtual string Text
    {
        get => _text;
        set => _text = value ?? string.Empty;
    }

    protected override ControlCollection CreateControlCollection()
    {
        return new EmptyControlCollection(this);
    }

    /// <devdoc>
    ///    <para>Saves any state that was modified after mark.</para>
    /// </devdoc>
    protected internal override void Render(HtmlTextWriter output)
    {
        output.Write(_text);
    }
}

