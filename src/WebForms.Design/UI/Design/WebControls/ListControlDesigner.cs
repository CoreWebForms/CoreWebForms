// MIT License.

using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Runtime;
using System.Web.UI.WebControls;

namespace System.Web.UI.Design.WebControls;

public class ListControlDesigner : DataBoundControlDesigner
{
    [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
    public ListControlDesigner() { }

    public override DesignerActionListCollection ActionLists { get; }
    public string DataTextField { get; set; }
    public string DataValueField { get; set; }
    protected override bool UseDataSourcePickerActionList { get; }

    public override string GetDesignTimeHtml() => throw new NotImplementedException();
    public new IEnumerable GetResolvedSelectedDataSource() => throw new NotImplementedException();
    public new object GetSelectedDataSource() => throw new NotImplementedException();
    public override void Initialize(IComponent component) => throw new NotImplementedException();
    public virtual void OnDataSourceChanged() => throw new NotImplementedException();
    protected override void DataBind(BaseDataBoundControl dataBoundControl) => throw new NotImplementedException();
    [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
    protected override void OnDataSourceChanged(bool forceUpdateView) => throw new NotImplementedException();
    protected override void PreFilterProperties(IDictionary properties) => throw new NotImplementedException();
}
