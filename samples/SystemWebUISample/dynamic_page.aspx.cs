// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Web.UI;

namespace SystemWebUISample.Pages;

public class DynamicPage : Page
{
    protected string TestValue1 = "Hello there!";

    protected string GetText(string value)
        => $"{value}: {GetCount(value)}";

    private int GetCount(string value)
    {
        if (ViewState[value] is int count)
        {
            ViewState[value] = ++count;
            return count;
        }
        else
        {
            ViewState[value] = 1;
            return 1;
        }
    }
}
