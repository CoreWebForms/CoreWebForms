using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Web.UI;

public class Control : IDisposable
{
    public virtual Control? Parent { get; }

    public virtual bool Visible { get; set; }

    protected internal virtual void Render(HtmlTextWriter writer)
        => RenderChildren(writer);

    protected internal virtual void RenderChildren(HtmlTextWriter writer)
    {
    }

    public virtual void Dispose()
    {
    }
}
