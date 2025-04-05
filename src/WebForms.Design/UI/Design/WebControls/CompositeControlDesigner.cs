// MIT License.

using System.ComponentModel;

namespace System.Web.UI.Design.WebControls;

public class CompositeControlDesigner : ControlDesigner
{
    public CompositeControlDesigner() { }

    public override string GetDesignTimeHtml() { throw new NotImplementedException(); }
    public override void Initialize(IComponent component) { throw new NotImplementedException(); }
    protected virtual void CreateChildControls() { throw new NotImplementedException(); }
}
