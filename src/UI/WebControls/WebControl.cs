namespace System.Web.UI.WebControls;

public class WebControl : Control
{
    public WebControl()
        : this(HtmlTextWriterTag.Span)
    {
    }

    public WebControl(HtmlTextWriterTag tag)
    {
    }
}
