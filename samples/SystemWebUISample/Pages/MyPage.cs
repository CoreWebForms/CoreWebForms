// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Web.UI;

namespace SystemWebUISample.Pages;

public class MyPage : Page
{
    protected void Button2_Click(object sender, EventArgs e)
    {
        Debugger.Break();
    }
}
