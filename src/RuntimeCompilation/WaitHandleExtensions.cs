// MIT License.

namespace Microsoft.AspNetCore.SystemWebAdapters.UI.RuntimeCompilation;

internal static class WaitHandleExtensions
{
    public static async Task WaitAsync(this WaitHandle handle, CancellationToken token)
    {
        var tcs = new TaskCompletionSource();

        using (new ThreadPoolRegistration(handle, tcs))
        using (token.Register(state => ((TaskCompletionSource)state!).TrySetCanceled(), tcs, useSynchronizationContext: false))
        {
            await tcs.Task.ConfigureAwait(false);
        }
    }

    private sealed class ThreadPoolRegistration : IDisposable
    {
        private readonly RegisteredWaitHandle _registeredWaitHandle;

        public ThreadPoolRegistration(WaitHandle handle, TaskCompletionSource tcs)
        {
            _registeredWaitHandle = ThreadPool.RegisterWaitForSingleObject(handle,
                (state, timedOut) => ((TaskCompletionSource)state!).TrySetResult(), tcs,
                Timeout.InfiniteTimeSpan, executeOnlyOnce: true);
        }

        void IDisposable.Dispose() => _registeredWaitHandle.Unregister(null);
    }
}
