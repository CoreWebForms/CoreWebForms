// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Web.UI;
using Microsoft.AspNetCore.SystemWebAdapters.UI;

namespace SystemWebUISample.Pages;

[AspxPage("/Test.aspx")]
public sealed class TestPage : Page
{
    private void Page_Load(object sender, EventArgs e)
    {
        Controls.Add(new LiteralControl("hello"));
    }
}
