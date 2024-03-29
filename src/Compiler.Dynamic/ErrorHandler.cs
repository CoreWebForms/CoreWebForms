// MIT License.

using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SystemWebAdapters;

namespace WebForms.Compiler.Dynamic;

internal sealed class ErrorHandler : HttpTaskAsyncHandler
{
    private readonly Exception _error;

    public ErrorHandler(Exception e) => _error = e;

    public override bool IsReusable => true;

    public override async Task ProcessRequestAsync(System.Web.HttpContext context)
    {
        if (_error is RoslynCompilationException r)
        {
            await context.AsAspNetCore().Response.WriteAsJsonAsync(r.Error).ConfigureAwait(true);
        }
        else
        {
            throw _error;
        }
    }
}
