// MIT License.

#nullable enable

using System.Diagnostics;
using System.Web.Util;

namespace System.Web;

/// <devdoc>
///    <para>[To be supplied.]</para>
/// </devdoc>
public delegate IAsyncResult BeginEventHandler(object sender, EventArgs e, AsyncCallback cb, object extraData);

/// <devdoc>
///    <para>[To be supplied.]</para>
/// </devdoc>
public delegate void EndEventHandler(IAsyncResult ar);

// Represents an event handler using TAP (Task Asynchronous Pattern).
public delegate Task TaskEventHandler(object sender, EventArgs e);

internal static class SynchronizationContextUtil
{
    internal const SynchronizationContextMode CurrentMode = SynchronizationContextMode.Normal;

    public static void ProhibitVoidAsyncOperations(this SynchronizationContext context)
    {
    }

    // Throws the exception that faulted a Task, similar to what 'await' would have done.
    // Useful for synchronous methods which have a Task instance they know to be already completed
    // and where they want to let the exception propagate upward.
    public static void ThrowIfFaulted(this Task task)
    {
        Debug.Assert(task.IsCompleted, "The Task passed to this method must be marked as completed so that this method doesn't block.");
        task.GetAwaiter().GetResult();
    }

    // Gets a WithinCancellableCallbackTaskAwaiter from a Task.
    public static WithinCancellableCallbackTaskAwaitable WithinCancellableCallback(this Task task, HttpContext context)
    {
        return new WithinCancellableCallbackTaskAwaitable(context, task.GetAwaiter());
    }

    public static void PostAsync(this SynchronizationContext context, Func<object, Task> func, object state)
        => throw new NotImplementedException();

    public static IAsyncDisposable EnableAsyncVoidOperations(this SynchronizationContext? context)
    {
        return new Empty();

#if PORT_ASYNC_VOID_OPERATIONS
        var newContext = new AsyncVoidSynchronizationContext(context);

        SynchronizationContext.SetSynchronizationContext(context);

        return newContext;
#endif
    }

    private class Empty : IAsyncDisposable
    {
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    internal static void ValidateMode(SynchronizationContextMode currentMode, SynchronizationContextMode requiredMode, string specificErrorMessage)
    {
        if (currentMode != requiredMode)
        {
            throw new InvalidOperationException(specificErrorMessage);
        }
    }

    // Adapted from https://www.meziantou.net/awaiting-an-async-void-method-in-dotnet.htm
    private sealed class AsyncVoidSynchronizationContext : SynchronizationContext, IAsyncDisposable
    {
        private static readonly SynchronizationContext s_default = new();

        private readonly SynchronizationContext _innerSynchronizationContext;
        private readonly TaskCompletionSource _tcs = new();

        private int _startedOperationCount;

        public AsyncVoidSynchronizationContext(SynchronizationContext? innerContext)
        {
            _innerSynchronizationContext = innerContext ?? s_default;
        }

        public override void OperationStarted()
        {
            Interlocked.Increment(ref _startedOperationCount);
        }

        public override void OperationCompleted()
        {
            if (Interlocked.Decrement(ref _startedOperationCount) == 0)
            {
                _tcs.TrySetResult();
            }
        }

        public override void Post(SendOrPostCallback d, object? state)
        {
            Interlocked.Increment(ref _startedOperationCount);

            try
            {
                _innerSynchronizationContext.Post(s =>
                {
                    try
                    {
                        d(s);
                    }
                    catch (Exception ex)
                    {
                        _tcs.TrySetException(ex);
                    }
                    finally
                    {
                        OperationCompleted();
                    }
                }, state);
            }
            catch (Exception ex)
            {
                _tcs.TrySetException(ex);
            }
        }

        public override void Send(SendOrPostCallback d, object? state)
        {
            try
            {
                _innerSynchronizationContext.Send(d, state);
            }
            catch (Exception ex)
            {
                _tcs.TrySetException(ex);
            }
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            await _tcs.Task.ConfigureAwait(false);
            SetSynchronizationContext(_innerSynchronizationContext);
        }
    }
}
