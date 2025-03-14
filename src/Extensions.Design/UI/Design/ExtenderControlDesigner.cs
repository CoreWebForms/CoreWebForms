// MIT License.

using System.Collections;
using System.ComponentModel;
using System.Runtime;

namespace System.Web.UI.Design;

public class ExtenderControlDesigner : ControlDesigner, IControlDesigner
{
    public ExtenderControlDesigner();

    protected override bool Visible { get; }

    public override string GetDesignTimeHtml();
    public override void Initialize(IComponent component);
    protected override void Dispose(bool disposing);
    protected override void PreFilterProperties(IDictionary properties);
}
