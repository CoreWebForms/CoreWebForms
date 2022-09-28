// MIT License.

#nullable disable

namespace System.Web.UI;

internal sealed class PageAsyncTaskManager
{

    private bool _executeTasksAsyncHasCompleted;
    private readonly Queue<IPageAsyncTask> _registeredTasks = new Queue<IPageAsyncTask>();

    public void EnqueueTask(IPageAsyncTask task)
    {
        if (_executeTasksAsyncHasCompleted)
        {
            // don't allow multiple calls to the execution routine
            throw new InvalidOperationException(SR.GetString(SR.PageAsyncManager_CannotEnqueue));
        }

        _registeredTasks.Enqueue(task);
    }

    public async Task ExecuteTasksAsync(object sender, EventArgs e, CancellationToken cancellationToken)
    {
        try
        {
            while (_registeredTasks.Count > 0)
            {
                // if canceled, propagate exception to caller and stop executing tasks
                cancellationToken.ThrowIfCancellationRequested();

#if PORT_REQUEST_NOTIFIER
                // if request finished, stop executing tasks
                if (requestCompletedNotifier.IsRequestCompleted)
                {
                    return;
                }
#endif

                // execute this task
                var task = _registeredTasks.Dequeue();
                await using (SynchronizationContext.Current.EnableAsyncVoidOperations())
                {
                    await task.ExecuteAsync(sender, e, cancellationToken).ConfigureAwait(true);
                }
            }
        }
        finally
        {
            _executeTasksAsyncHasCompleted = true;
        }
    }

}
