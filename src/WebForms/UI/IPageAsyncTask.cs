// MIT License.

#nullable disable

namespace System.Web.UI;

internal interface IPageAsyncTask
{
    Task ExecuteAsync(object sender, EventArgs e, CancellationToken cancellationToken);
}
