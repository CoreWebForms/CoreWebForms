using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace System.Web.UI;

public class Control : IDisposable
{
    public Control? Parent { get; }

    public bool Visible { get; set; }

    public ControlCollection Controls { get; } = new();

    public virtual void RenderControl(HtmlTextWriter writer)
        => Render(writer);

    protected internal virtual void Render(HtmlTextWriter writer)
        => RenderChildren(writer);

    protected internal virtual void RenderChildren(HtmlTextWriter writer)
    {
        foreach (Control child in Controls)
        {
            child.RenderControl(writer);
        }
    }

    public virtual void Dispose()
    {
    }
}
