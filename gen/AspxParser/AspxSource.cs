// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.AspNetCore.SystemWebAdapters.UI.PageParser;

public class AspxSource : IAspxSource
{
    public static AspxSource Empty { get; } = new AspxSource("", "");

    public string Name { get; }
    public string Text { get; }

    public AspxSource(string name, string text)
    {
        Name = name;
        Text = text;
    }
}
