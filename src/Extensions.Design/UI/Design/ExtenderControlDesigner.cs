// MIT License.

using System.Collections;
using System.ComponentModel;

namespace System.Web.UI.Design;

public class ExtenderControlDesigner : ControlDesigner, IControlDesigner
{
    public ExtenderControlDesigner() { }

    protected override bool Visible { get; }

    bool IControlDesigner.Visible => throw new NotImplementedException();

    public override string GetDesignTimeHtml() => throw new NotImplementedException();
    public override void Initialize(IComponent component) => throw new NotImplementedException();

    public void UpdateDesignTimeHtml()
    {
        throw new NotImplementedException();
    }

    protected  void Dispose(bool disposing) => throw new NotImplementedException();
    protected override void PreFilterProperties(IDictionary properties) => throw new NotImplementedException();

    string IControlDesigner.CreatePlaceHolderDesignTimeHtml()
    {
        throw new NotImplementedException();
    }
}

public class ControlDesigner 
{
    protected virtual bool Visible { get; }
    public virtual string GetDesignTimeHtml() => throw new NotImplementedException();
    public virtual void Initialize(IComponent component) => throw new NotImplementedException();
    protected virtual void PreFilterProperties(IDictionary properties) => throw new NotImplementedException();
}
