// MIT License.

using System.Collections;
using System.ComponentModel;

namespace System.Web.UI.Design;

public class ExtenderControlDesigner : ControlDesigner, IControlDesigner
{
    public ExtenderControlDesigner() => throw new NotImplementedException();
    protected override bool Visible { get; }

    bool IControlDesigner.Visible => throw new NotImplementedException();

    public override string GetDesignTimeHtml() => throw new NotImplementedException();
    public override void Initialize(IComponent component) => throw new NotImplementedException();
    protected override void Dispose(bool disposing) => throw new NotImplementedException();
    protected override void PreFilterProperties(IDictionary properties) => throw new NotImplementedException();

    string IControlDesigner.CreatePlaceHolderDesignTimeHtml()
    {
        throw new NotImplementedException();
    }
}
