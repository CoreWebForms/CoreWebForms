// MIT License.

using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Runtime;
//using System.Windows.Forms;

namespace System.Web.UI.Design;

public class ControlDesigner : HtmlControlDesigner
{
    public ControlDesigner() { }

    public override DesignerActionListCollection ActionLists { get; }
    public virtual bool AllowResize { get; }
    public virtual DesignerAutoFormatCollection AutoFormats { get; }
    [Obsolete("The recommended alternative is SetViewFlags(ViewFlags.DesignTimeHtmlRequiresLoadComplete, true). http://go.microsoft.com/fwlink/?linkid=14202")]
    public virtual bool DesignTimeHtmlRequiresLoadComplete { get; }
    public virtual string ID { get; set; }
    [Obsolete("The recommended alternative is to use Tag.SetDirty() and Tag.IsDirty. http://go.microsoft.com/fwlink/?linkid=14202")]
    public bool IsDirty { get; set; }
    [Obsolete("The recommended alternative is to inherit from ContainerControlDesigner instead and to use an EditableDesignerRegion. Regions allow for better control of the content in the designer. http://go.microsoft.com/fwlink/?linkid=14202")]
    public bool ReadOnly { get; set; }
    public virtual TemplateGroupCollection TemplateGroups { get; }
    public Control ViewControl { get; set; }
    public virtual bool ViewControlCreated { get; set; }
    protected virtual bool DataBindingsEnabled { get; }
    protected ControlDesignerState DesignerState { get; }
    [Obsolete("Error: This property can no longer be referenced, and is included to support existing compiled applications. The design-time element view architecture is no longer used. http://go.microsoft.com/fwlink/?linkid=14202", true)]
    protected object DesignTimeElementView { get; }
    protected bool InTemplateMode { get; }
    protected WebFormsRootDesigner RootDesigner { get; }
    protected IControlDesignerTag Tag { get; }
    protected virtual bool UsePreviewControl { get; }
    protected virtual bool Visible { get; }
    protected internal virtual bool HidePropertiesInTemplateMode { get; }

    public static DesignTimeResourceProviderFactory GetDesignTimeResourceProviderFactory(IServiceProvider serviceProvider) => throw new NotImplementedException();
    public static ViewRendering GetViewRendering(ControlDesigner designer) => throw new NotImplementedException();
    public static ViewRendering GetViewRendering(Control control) => throw new NotImplementedException();
    [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
    public static void InvokeTransactedChange(IComponent component, TransactedChangeCallback callback, object context, string description) => throw new NotImplementedException();
    public static void InvokeTransactedChange(IComponent component, TransactedChangeCallback callback, object context, string description, MemberDescriptor member) => throw new NotImplementedException();
    public static void InvokeTransactedChange(IServiceProvider serviceProvider, IComponent component, TransactedChangeCallback callback, object context, string description, MemberDescriptor member) => throw new NotImplementedException();
    public Rectangle GetBounds() => throw new NotImplementedException();
    public virtual string GetDesignTimeHtml() => throw new NotImplementedException();
    [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
    public virtual string GetDesignTimeHtml(DesignerRegionCollection regions) => throw new NotImplementedException();
    public virtual string GetEditableDesignerRegionContent(EditableDesignerRegion region) => throw new NotImplementedException();
    [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
    public virtual string GetPersistenceContent() => throw new NotImplementedException();
    [Obsolete("The recommended alternative is GetPersistenceContent(). http://go.microsoft.com/fwlink/?linkid=14202")]
    [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
    public virtual string GetPersistInnerHtml() => throw new NotImplementedException();
    public ViewRendering GetViewRendering() => throw new NotImplementedException();
    public override void Initialize(IComponent component) => throw new NotImplementedException();
    public void Invalidate() => throw new NotImplementedException();
    public void Invalidate(Rectangle rectangle) => throw new NotImplementedException();
    [Obsolete("The recommended alternative is DataBindings.Contains(string). The DataBindings collection allows more control of the databindings associated with the control. http://go.microsoft.com/fwlink/?linkid=14202")]
    public bool IsPropertyBound(string propName) => throw new NotImplementedException();
    public void Localize(IDesignTimeResourceWriter resourceWriter) => throw new NotImplementedException();
    public virtual void OnAutoFormatApplied(DesignerAutoFormat appliedAutoFormat) => throw new NotImplementedException();
    public virtual void OnComponentChanged(object sender, ComponentChangedEventArgs ce) => throw new NotImplementedException();
    public virtual void OnComponentChanging(object sender, ComponentChangingEventArgs ce) => throw new NotImplementedException();
    [Obsolete("Use of this method is not recommended because resizing is handled by the OnComponentChanged() method. http://go.microsoft.com/fwlink/?linkid=14202")]
    [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
    public void RaiseResizeEvent() => throw new NotImplementedException();
    public void RegisterClone(object original, object clone) => throw new NotImplementedException();
    public virtual void SetEditableDesignerRegionContent(EditableDesignerRegion region, string content) => throw new NotImplementedException();
    public virtual void UpdateDesignTimeHtml() => throw new NotImplementedException();
    [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
    protected string CreateErrorDesignTimeHtml(string errorMessage) => throw new NotImplementedException();
    protected string CreateErrorDesignTimeHtml(string errorMessage, Exception e) => throw new NotImplementedException();
    [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
    protected string CreatePlaceHolderDesignTimeHtml() => throw new NotImplementedException();
    protected string CreatePlaceHolderDesignTimeHtml(string instruction) => throw new NotImplementedException();
    protected virtual Control CreateViewControl() => throw new NotImplementedException();
    protected virtual string GetEmptyDesignTimeHtml() => throw new NotImplementedException();
    protected virtual string GetErrorDesignTimeHtml(Exception e) => throw new NotImplementedException();
    [Obsolete("The recommended alternative is to handle the Changed event on the DataBindings collection. The DataBindings collection allows more control of the databindings associated with the control. http://go.microsoft.com/fwlink/?linkid=14202")]
    protected override void OnBindingsCollectionChanged(string propName) => throw new NotImplementedException();
    protected virtual void OnClick(DesignerRegionMouseEventArgs e) => throw new NotImplementedException();
    [Obsolete("The recommended alternative is OnComponentChanged(). OnComponentChanged is called when any property of the control is changed. http://go.microsoft.com/fwlink/?linkid=14202")]
    protected virtual void OnControlResize() => throw new NotImplementedException();
    protected virtual void OnPaint(PaintEventArgs e) => throw new NotImplementedException();
    protected override void PreFilterProperties(IDictionary properties) => throw new NotImplementedException();
    protected void SetRegionContent(EditableDesignerRegion region, string content) => throw new NotImplementedException();
    protected void SetViewFlags(ViewFlags viewFlags, bool setFlag) => throw new NotImplementedException();
}
