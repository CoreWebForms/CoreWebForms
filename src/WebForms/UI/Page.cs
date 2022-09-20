// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Web.UI.Features;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

// TODO: Remove once implemented
#pragma warning disable CA1822 // Mark members as static

namespace System.Web.UI;
public class Page : TemplateControl, IHttpAsyncHandler
{
    private ClientScriptManager? _clientScriptManager;

    public Page()
    {
    }

    bool IHttpHandler.IsReusable => false;

    IAsyncResult IHttpAsyncHandler.BeginProcessRequest(HttpContext context, AsyncCallback cb, object? extraData)
        => TaskAsyncHelper.BeginTask(() => ProcessAsync(context), cb, extraData);

    void IHttpAsyncHandler.EndProcessRequest(IAsyncResult result)
        => TaskAsyncHelper.EndTask(result);

    void IHttpHandler.ProcessRequest(HttpContext context)
        => throw new InvalidOperationException();

    internal Task ProcessAsync(HttpContext context)
    {
        context.Response.ContentType = "text/html";

        if (Features.Get<HttpContext>() is { })
        {
            throw new InvalidOperationException("Page has already been processed.");
        }

        InitializeComponents();

        var events = Features.Get<IPageEvents>()!;

        Features.Set(context);

        events.OnPageLoad(this);

        using var writer = new HtmlTextWriter(context.Response.Output);

        Render(writer);

        return Task.CompletedTask;
    }

    public HtmlForm? Form => Features.Get<IFormWriterFeature>()?.Form;

    public ClientScriptManager ClientScript => _clientScriptManager ??= new ClientScriptManager(this);

    public bool IsPostBackEventControlRegistered { get; internal set; }

    internal Control? AutoPostBackControl { get; set; }

    protected virtual void InitializeComponents()
    {
    }

    internal void RegisterEnabledControl(TextBox textBox)
    {
    }

    internal void RegisterWebFormsScript()
    {
    }

    internal void RegisterPostBackScript()
    {
    }

    internal void RegisterFocusScript()
    {
    }

    internal void Validate(string validationGroup)
    {
        throw new NotImplementedException();
    }
}
