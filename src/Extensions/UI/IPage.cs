// MIT License.

namespace System.Web.UI;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Web.UI.HtmlControls;

internal interface IPage
{
    string AppRelativeVirtualPath
    {
        get;
    }

    IDictionary<String, String> HiddenFieldsToRender
    {
        get;
    }

    IClientScriptManager ClientScript
    {
        get;
    }

    bool EnableEventValidation
    {
        get;
    }

    IHtmlForm Form
    {
        get;
    }

    HtmlHead Header
    {
        get;
    }

    bool IsPostBack
    {
        get;
    }

    bool IsValid
    {
        get;
    }

    IDictionary Items
    {
        get;
    }

    HttpRequestBase Request
    {
        get;
    }

#if PORT_IPAGE
    HttpResponseInternalBase Response
    {
        get;
    }
#endif

    HttpServerUtilityBase Server
    {
        get;
    }

    string Title
    {
        get;
        set;
    }

    event EventHandler Error;
    event EventHandler InitComplete;
    event EventHandler LoadComplete;
    event EventHandler PreRender;
    event EventHandler PreRenderComplete;

    void RegisterRequiresViewStateEncryption();
    void SetFocus(Control control);
    void SetFocus(string clientID);
    void SetPostFormRenderDelegate(RenderMethod renderMethod);
    void SetRenderMethodDelegate(RenderMethod renderMethod);
    void Validate(string validationGroup);
    void VerifyRenderingInServerForm(Control control);
}
