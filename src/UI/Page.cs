using System.Web.UI.Features;

namespace System.Web.UI;

public class Page : TemplateControl, IHttpAsyncHandler
{
    private readonly IPageEvents _events;

    public Page()
    {
        _events = new PageEvents(GetType());

        Features.Set<Page>(this);
    }

    bool IHttpHandler.IsReusable => false;

    IAsyncResult IHttpAsyncHandler.BeginProcessRequest(HttpContext context, AsyncCallback cb, object? extraData)
        => TaskAsyncHelper.BeginTask(() => ProcessAsync(context), cb, extraData);

    void IHttpAsyncHandler.EndProcessRequest(IAsyncResult result)
        => TaskAsyncHelper.EndTask(result);

    void IHttpHandler.ProcessRequest(HttpContext context)
        => throw new InvalidOperationException();

    private Task ProcessAsync(HttpContext context)
    {
        Features.Set(context);
        this.EnableUniqueIdGenerator();

        _events.OnPageLoad(this);

        using var writer = new HtmlTextWriter(context.Response.Output);

        Render(writer);

        return Task.CompletedTask;
    }

    private const string HiddenClassName = "aspNetHidden";

    internal void BeginFormRender(HtmlTextWriter writer, string? formUniqueID)
    {
        writer.WriteBeginTag("div");
        writer.WriteAttribute("class", HiddenClassName);
        writer.WriteEndTag("div");
    }
}