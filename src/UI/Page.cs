namespace System.Web.UI;

public class Page : TemplateControl, IHttpHandler
{
    bool IHttpHandler.IsReusable => false;

    void IHttpHandler.ProcessRequest(HttpContext context)
    {
    }
}