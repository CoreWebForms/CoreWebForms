// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

namespace System.Web.UI;

internal interface IPageAsyncTask
{
    Task ExecuteAsync(object sender, EventArgs e, CancellationToken cancellationToken);
}
