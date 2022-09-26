// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;

namespace System.Web.UI.WebControls;

public class PlaceHolder : Control
{

    /// <devdoc>
    ///    <para>Gets and sets a value indicating whether theme is enabled.</para>
    /// </devdoc>
    [
    Browsable(true)
    ]
    public override bool EnableTheming
    {
        get
        {
            return base.EnableTheming;
        }
        set
        {
            base.EnableTheming = value;
        }
    }
}
