namespace System.Web.UI;

public class Page : IHttpHandler
{
    bool IHttpHandler.IsReusable => false;

    void IHttpHandler.ProcessRequest(HttpContext context)
    {
    }
}