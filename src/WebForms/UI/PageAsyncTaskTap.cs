// MIT License.

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
