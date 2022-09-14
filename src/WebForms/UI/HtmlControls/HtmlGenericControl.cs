// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web.UI.HtmlControls;

public class HtmlGenericControl : HtmlContainerControl
{
    public HtmlGenericControl()
      : this("span")
    {
    }

    public HtmlGenericControl(string tag)
        : base(tag)
    {
    }
}
