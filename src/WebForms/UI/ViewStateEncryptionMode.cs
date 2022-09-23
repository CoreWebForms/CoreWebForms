// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web.UI;

/// <devdoc>
///    <para>Specifies whether viewstate is encrypted.</para>
/// </devdoc>
public enum ViewStateEncryptionMode
{

    /// <devdoc>
    ///    <para>The viewstate will be encrypted if a control requests it.<para>
    /// </devdoc>
    Auto = 0,

    /// <devdoc>
    ///    <para>The viewstate will always be encypted.</para>
    /// </devdoc>
    Always = 1,

    /// <devdoc>
    ///    <para>The viewstate will never be encypted, even if a control requests it.</para>
    /// </devdoc>
    Never = 2,
}
