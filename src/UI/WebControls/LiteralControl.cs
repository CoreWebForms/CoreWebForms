// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web.UI;

public class LiteralControl : Control, ITextControl
{
    private string? _text;

    public LiteralControl()
    {
    }

    public LiteralControl(string text)
    {
        Text = text;
    }

    protected internal override void Render(HtmlTextWriter output)
    {
        output.Write(Text);
    }

    public string Text
    {
        get => _text ?? string.Empty;
        set => _text = value;
    }
}
