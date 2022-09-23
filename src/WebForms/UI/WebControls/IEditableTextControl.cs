// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web.UI.WebControls;
public interface IEditableTextControl : ITextControl
{
    /// <devdoc>
    ///     Raised when the text changes.
    /// </devdoc>
    event EventHandler TextChanged;
}
