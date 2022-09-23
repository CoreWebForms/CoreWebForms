// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web.UI;

// Represents an IPageAsyncTask that follows APM (Begin / End)

internal sealed class PageAsyncTaskApm : IPageAsyncTask
{

    private readonly BeginEventHandler _beginHandler;
    private readonly EndEventHandler _endHandler;
    private readonly object _state;

    public PageAsyncTaskApm(BeginEventHandler beginHandler, EndEventHandler endHandler, object state)
    {
        _beginHandler = beginHandler;
        _endHandler = endHandler;
        _state = state;
    }

    public async Task ExecuteAsync(object sender, EventArgs e, CancellationToken cancellationToken)
    {
        // The CancellationToken is ignored in APM since every call to Begin must be matched by
        // a call to End, otherwise memory leaks and other badness can occur.

        // The reason we don't use TaskFactory.FromAsync is that we need the end handler to execute
        // within the current synchronization context, but FromAsync doesn't make that guarantee.

        // The callback that marks the TaskCompletionSource as complete can execute synchronously or asynchronously.
        TaskCompletionSource<object> taskCompletionSource = new TaskCompletionSource<object>();
        IAsyncResult asyncResult = _beginHandler(sender, e, _ => { taskCompletionSource.SetResult(null); }, _state);

        await taskCompletionSource.Task.ConfigureAwait(false);
        _endHandler(asyncResult);
    }

}
