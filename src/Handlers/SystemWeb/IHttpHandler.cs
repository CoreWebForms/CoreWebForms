// MIT License.

namespace System.Web;

public interface IHttpHandler
{
    void ProcessRequest(HttpContext context);

    bool IsReusable { get; }
}
