// MIT License.

using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;

namespace System.Web.UI.Design
{
    public class HtmlControlDesigner : ComponentDesigner
    {
        public HtmlControlDesigner() { }

        [Obsolete("The recommended alternative is ControlDesigner.Tag. http://go.microsoft.com/fwlink/?linkid=14202")]
        public IHtmlControlDesignerBehavior Behavior { get; set; }
        public DataBindingCollection DataBindings { get; }
        public ExpressionBindingCollection Expressions { get; }
        [Obsolete("Use of this property is not recommended because code serialization is not supported. http://go.microsoft.com/fwlink/?linkid=14202")]
        public virtual bool ShouldCodeSerialize { get; set; }
        [Obsolete("Error: This property can no longer be referenced, and is included to support existing compiled applications. The design-time element may not always provide access to the element in the markup. There are alternate methods on WebFormsRootDesigner for handling client script and controls. http://go.microsoft.com/fwlink/?linkid=14202", true)]
        protected object DesignTimeElement { get; }

        public override void Initialize(IComponent component) => throw new NotImplementedException();
        public virtual void OnSetParent() => throw new NotImplementedException();
        protected override void Dispose(bool disposing) => throw new NotImplementedException();
        [Obsolete("The recommended alternative is ControlDesigner.Tag. http://go.microsoft.com/fwlink/?linkid=14202")]
        protected virtual void OnBehaviorAttached() => throw new NotImplementedException();
        [Obsolete("The recommended alternative is ControlDesigner.Tag. http://go.microsoft.com/fwlink/?linkid=14202")]
        protected virtual void OnBehaviorDetaching() => throw new NotImplementedException();
        [Obsolete("The recommended alternative is to handle the Changed event on the DataBindings collection. The DataBindings collection allows more control of the databindings associated with the control. http://go.microsoft.com/fwlink/?linkid=14202")]
        protected virtual void OnBindingsCollectionChanged(string propName) => throw new NotImplementedException();
        protected override void PreFilterEvents(IDictionary events) => throw new NotImplementedException();
        protected override void PreFilterProperties(IDictionary properties) => throw new NotImplementedException();
    }
}
