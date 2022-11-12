// MIT License.

using System.Web;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.SystemWebAdapters.UI.RuntimeCompilation;

internal sealed class ErrorHandler : HttpTaskAsyncHandler
{
    private readonly Exception _error;

    public ErrorHandler(Exception e) => _error = e;

    public override bool IsReusable => true;

    public override async Task ProcessRequestAsync(System.Web.HttpContext context)
    {
        if (_error is RoslynCompilationException r)
        {
            await context.AsCore().Response.WriteAsJsonAsync(r.Error).ConfigureAwait(true);
        }
        else
        {
            throw _error;
        }
    }
}
