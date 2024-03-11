//MIT license

using System.Collections;

namespace System.Web.UI.WebControls.Adapters; 
public class DataBoundControlAdapter : WebControlAdapter {

    protected new DataBoundControl Control {
        get {
            return (DataBoundControl)base.Control;
        }
    }

    protected internal virtual void PerformDataBinding(IEnumerable data) {
        Control.PerformDataBinding(data);
    }
}
