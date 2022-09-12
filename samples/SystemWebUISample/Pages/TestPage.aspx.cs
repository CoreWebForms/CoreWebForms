// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Web.UI;

namespace SystemWebUISample.Pages;

public sealed class TestPage : Page
{
    private void Page_Load(object sender, EventArgs e)
    {
        Controls.Add(new LiteralControl("hello"));
    }
}
