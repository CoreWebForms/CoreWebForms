// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web;

internal static class SynchronizationContextExtensions
{
    public static IAsyncDisposable EnableAsyncVoidOperations(this SynchronizationContext? context)
    {
        var newContext = new AsyncVoidSynchronizationContext(context);

        SynchronizationContext.SetSynchronizationContext(context);

        return newContext;
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
