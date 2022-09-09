namespace System.Web.UI;

public class Page : TemplateControl, IHttpAsyncHandler
{
    bool IHttpHandler.IsReusable => false;

    IAsyncResult IHttpAsyncHandler.BeginProcessRequest(HttpContext context, AsyncCallback cb, object? extraData)
        => TaskAsyncHelper.BeginTask(() => ProcessAsync(context), cb, extraData);

    void IHttpAsyncHandler.EndProcessRequest(IAsyncResult result)
        => TaskAsyncHelper.EndTask(result);

    void IHttpHandler.ProcessRequest(HttpContext context)
        => throw new InvalidOperationException();

    private Task ProcessAsync(HttpContext context)
    {
        using var writer = new HtmlTextWriter(context.Response.Output);

        Render(writer);

        return Task.CompletedTask;
    }
}