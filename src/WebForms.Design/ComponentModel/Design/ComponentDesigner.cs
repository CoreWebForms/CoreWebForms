// MIT License.

using System.Collections;
using System.Runtime;

namespace System.ComponentModel.Design;
public class ComponentDesigner : ITreeDesigner, IDesigner, IDisposable, IDesignerFilter, IComponentInitializer
{
    [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
    public ComponentDesigner() { }

    ~ComponentDesigner() { }

    public virtual DesignerActionListCollection ActionLists { get; }
    public virtual ICollection AssociatedComponents { get; }
    public IComponent Component { get; }
    public virtual DesignerVerbCollection Verbs { get; }
    protected virtual InheritanceAttribute InheritanceAttribute { get; }
    protected bool Inherited { get; }
    protected virtual IComponent ParentComponent { get; }
    protected ShadowPropertyCollection ShadowProperties { get; }

    ICollection ITreeDesigner.Children => throw new NotImplementedException();

    IDesigner ITreeDesigner.Parent => throw new NotImplementedException();

    IComponent IDesigner.Component => throw new NotImplementedException();

    DesignerVerbCollection IDesigner.Verbs => throw new NotImplementedException();

    public void Dispose() => throw new NotImplementedException();
    public virtual void DoDefaultAction()=> throw new NotImplementedException();
    public virtual void Initialize(IComponent component) => throw new NotImplementedException();
    [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
    public virtual void InitializeExistingComponent(IDictionary defaultValues) => throw new NotImplementedException();
    public virtual void InitializeNewComponent(IDictionary defaultValues) => throw new NotImplementedException();
    [Obsolete("This method has been deprecated. Use InitializeExistingComponent instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
    public virtual void InitializeNonDefault() => throw new NotImplementedException();
    [Obsolete("This method has been deprecated. Use InitializeNewComponent instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
    public virtual void OnSetComponentDefaults() => throw new NotImplementedException();
    protected virtual void Dispose(bool disposing) => throw new NotImplementedException();
    protected virtual object GetService(Type serviceType) => throw new NotImplementedException();
    [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
    protected InheritanceAttribute InvokeGetInheritanceAttribute(ComponentDesigner toInvoke) => throw new NotImplementedException();
    protected virtual void PostFilterAttributes(IDictionary attributes) => throw new NotImplementedException();
    protected virtual void PostFilterEvents(IDictionary events) => throw new NotImplementedException();
    protected virtual void PostFilterProperties(IDictionary properties) => throw new NotImplementedException();
    protected virtual void PreFilterAttributes(IDictionary attributes) => throw new NotImplementedException();
    protected virtual void PreFilterEvents(IDictionary events) => throw new NotImplementedException();
    protected virtual void PreFilterProperties(IDictionary properties) => throw new NotImplementedException();
    protected void RaiseComponentChanged(MemberDescriptor member, object oldValue, object newValue) => throw new NotImplementedException();
    protected void RaiseComponentChanging(MemberDescriptor member) => throw new NotImplementedException();

    void IDisposable.Dispose()
    {
        throw new NotImplementedException();
    }

    void IDesigner.DoDefaultAction()
    {
        throw new NotImplementedException();
    }

    void IDesigner.Initialize(IComponent component)
    {
        throw new NotImplementedException();
    }

    void IDesignerFilter.PostFilterAttributes(IDictionary attributes)
    {
        throw new NotImplementedException();
    }

    void IDesignerFilter.PostFilterEvents(IDictionary events)
    {
        throw new NotImplementedException();
    }

    void IDesignerFilter.PostFilterProperties(IDictionary properties)
    {
        throw new NotImplementedException();
    }

    void IDesignerFilter.PreFilterAttributes(IDictionary attributes)
    {
        throw new NotImplementedException();
    }

    void IDesignerFilter.PreFilterEvents(IDictionary events)
    {
        throw new NotImplementedException();
    }

    void IDesignerFilter.PreFilterProperties(IDictionary properties)
    {
        throw new NotImplementedException();
    }


    protected sealed class ShadowPropertyCollection
    {
        public object this[string propertyName] { get { throw new NotImplementedException(); } set { } }

        public bool Contains(string propertyName) => throw new NotImplementedException();
    }
}
