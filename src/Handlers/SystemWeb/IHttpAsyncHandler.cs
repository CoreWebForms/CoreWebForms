// MIT License.

namespace System.Web;

public interface IHttpAsyncHandler : IHttpHandler
{
    IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object? extraData);

    void EndProcessRequest(IAsyncResult result);
}
