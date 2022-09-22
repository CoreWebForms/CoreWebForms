// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;

#nullable disable

namespace System.Web;
internal class HttpAsyncResult : IAsyncResult
{
    private readonly AsyncCallback _callback;

    private object _result;
    private Thread _threadWhichStartedOperation;

    /*
     * Constructor with pending result
     */
    internal HttpAsyncResult(AsyncCallback cb, object state)
    {
        _callback = cb;
        AsyncState = state;
        Status = RequestNotificationStatus.Continue;
    }

    /*
     * Constructor with known result
     */
    internal HttpAsyncResult(AsyncCallback cb, object state,
                             bool completed, object result, Exception error)
    {
        _callback = cb;
        AsyncState = state;

        IsCompleted = completed;
        CompletedSynchronously = completed;

        _result = result;
        Error = error;
        Status = RequestNotificationStatus.Continue;

        if (IsCompleted && _callback != null)
            _callback(this);
    }

    internal void SetComplete()
    {
        IsCompleted = true;
    }

    /*
     * Helper method to process completions
     */
    internal void Complete(bool synchronous, object result, Exception error, RequestNotificationStatus status)
    {
        if (Volatile.Read(ref _threadWhichStartedOperation) == Thread.CurrentThread)
        {
            // If we're calling Complete on the same thread which kicked off the operation, then
            // we ignore the 'synchronous' value that the caller provided to us since we know
            // for a fact that this is really a synchronous completion. This is only checked if
            // the caller calls the MarkCallToBeginMethod* routines below.
            synchronous = true;
        }

        IsCompleted = true;
        CompletedSynchronously = synchronous;
        _result = result;
        Error = error;
        Status = status;

        if (_callback != null)
            _callback(this);
    }

    internal void Complete(bool synchronous, object result, Exception error)
    {
        Complete(synchronous, result, error, RequestNotificationStatus.Continue);
    }


    /*
     * Helper method to implement End call to async method
     */
    internal object End()
    {
        if (Error != null)
            throw new HttpException(null, Error);

        return _result;
    }

    // If the caller needs to invoke an asynchronous method where the only way of knowing whether the
    // method actually completed synchronously is to inspect which thread the callback was invoked on,
    // then the caller should surround the asynchronous call with calls to the below Started / Completed
    // methods. The callback can compare the captured thread against the current thread to see if the
    // completion was synchronous. The caller calls the Completed method when unwinding so that the
    // captured thread can be cleared out, preventing an asynchronous invocation on the same thread
    // from being mistaken for a synchronous invocation.

    internal void MarkCallToBeginMethodStarted()
    {
        Thread originalThread = Interlocked.CompareExchange(ref _threadWhichStartedOperation, Thread.CurrentThread, null);
        Debug.Assert(originalThread == null, "Another thread already called MarkCallToBeginMethodStarted.");
    }

    internal void MarkCallToBeginMethodCompleted()
    {
        Thread originalThread = Interlocked.Exchange(ref _threadWhichStartedOperation, null);
        Debug.Assert(originalThread == Thread.CurrentThread, "This thread did not call MarkCallToBeginMethodStarted.");
    }

    //
    // Properties that are not part of IAsyncResult
    //

    internal Exception Error { get; private set; }

    internal RequestNotificationStatus Status { get; private set; }

    //
    // IAsyncResult implementation
    //

    public bool IsCompleted { get; private set; }
    public bool CompletedSynchronously { get; private set; }
    public object AsyncState { get; }
    public WaitHandle AsyncWaitHandle { get { return null; } } // wait not supported
}
