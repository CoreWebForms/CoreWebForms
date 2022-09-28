// MIT License.

using System.ComponentModel;

namespace System.Web;

public abstract class HttpTaskAsyncHandler : IHttpAsyncHandler, IHttpHandler
{
    public virtual bool IsReusable => false;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public virtual void ProcessRequest(HttpContext context) => throw new NotSupportedException($"{GetType()} cannot execute synchronously");

    public abstract Task ProcessRequestAsync(HttpContext context);

    IAsyncResult IHttpAsyncHandler.BeginProcessRequest(HttpContext context, AsyncCallback cb, object? extraData)
        => new TaskWithState(ProcessRequestAsync(context).ContinueWith(t => cb(t), TaskScheduler.Current), extraData);

    void IHttpAsyncHandler.EndProcessRequest(IAsyncResult result)
        => ((TaskWithState)result).Wait();

    private sealed class TaskWithState : IAsyncResult
    {
        private readonly IAsyncResult _task;

        public TaskWithState(Task task, object? asyncState)
        {
            _task = task;
            AsyncState = asyncState;
        }

        public void Wait() => ((Task)_task).Wait();

        public object? AsyncState { get; }

        public WaitHandle AsyncWaitHandle => _task.AsyncWaitHandle;

        public bool CompletedSynchronously => _task.CompletedSynchronously;

        public bool IsCompleted => _task.IsCompleted;
    }
}
