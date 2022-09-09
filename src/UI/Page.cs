using System.Reflection;

namespace System.Web.UI;

internal interface IPageEvents
{
    void OnPageLoad(Page page);
}

internal class PageEvents : IPageEvents
{
    private readonly Type _type;
    private readonly Action<object, EventArgs>? _onLoad;

    public PageEvents(Type type)
    {
        _type = type;

        var method = type.GetMethod("Page_Load", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (method is not null && method.ReturnType == typeof(void))
        {
            var parameters = method.GetParameters();

            if (parameters.Length == 0)
            {
                _onLoad = (object target, EventArgs o) => method.Invoke(target, null);
            }
            else if (parameters.Length == 2 && parameters[0].ParameterType == typeof(object) && parameters[1].ParameterType == typeof(EventArgs))
            {
                _onLoad = (object target, EventArgs o) => method.Invoke(target, new object[] { target, o });
            }
        }
    }

    public void OnPageLoad(Page page)
    {
        _onLoad?.Invoke(page, EventArgs.Empty);
    }
}

public class Page : TemplateControl, IHttpAsyncHandler
{
    private readonly IPageEvents _events;

    public Page()
    {
        _events = new PageEvents(GetType());
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
        _events.OnPageLoad(this);

        using var writer = new HtmlTextWriter(context.Response.Output);

        Render(writer);

        return Task.CompletedTask;
    }
}