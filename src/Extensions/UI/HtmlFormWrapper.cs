// MIT License.

using System.Diagnostics;
using System.Web.UI.HtmlControls;

namespace System.Web.UI;

internal sealed class HtmlFormWrapper : IHtmlForm
{
    private readonly HtmlForm _form;

    public HtmlFormWrapper(HtmlForm form)
    {
        Debug.Assert(form != null);
        _form = form;
    }

    #region IHtmlForm Members
    string IHtmlForm.ClientID
    {
        get
        {
            return _form.ClientID;
        }
    }

    string IHtmlForm.Method
    {
        get
        {
            return _form.Method;
        }
    }

    void IHtmlForm.RenderControl(HtmlTextWriter writer)
    {
        _form.RenderControl(writer);
    }

    void IHtmlForm.SetRenderMethodDelegate(RenderMethod renderMethod)
    {
        _form.SetRenderMethodDelegate(renderMethod);
    }
    #endregion
}
