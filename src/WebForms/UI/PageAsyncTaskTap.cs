// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web.UI;
internal sealed class PageAsyncTaskTap : IPageAsyncTask
{
    private readonly Func<CancellationToken, Task> _handler;

    public PageAsyncTaskTap(Func<CancellationToken, Task> handler)
    {
        _handler = handler;
    }

    public Task ExecuteAsync(object sender, EventArgs e, CancellationToken cancellationToken)
    {
        return _handler(cancellationToken);
    }
}
