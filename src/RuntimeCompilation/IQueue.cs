// MIT License.

namespace Microsoft.AspNetCore.SystemWebAdapters.UI.RuntimeCompilation;

internal interface IQueue
{
    void Add(Func<CancellationToken, Task> func);

    IAsyncEnumerable<Func<CancellationToken, Task>> GetItemsAsync(CancellationToken token);
}
