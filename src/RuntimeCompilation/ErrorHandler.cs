// MIT License.

using System.Web;

namespace Microsoft.AspNetCore.SystemWebAdapters.UI.RuntimeCompilation;

internal sealed class ErrorHandler : IHttpHandler
{
    private readonly Exception _error;

    public ErrorHandler(Exception e) => _error = e;

    public bool IsReusable => true;

    public void ProcessRequest(HttpContext context) => throw _error;
}
