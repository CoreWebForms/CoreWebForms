// MIT License.

using System.Collections;
using System.ComponentModel;
using System.Runtime;
using System.Web.UI.Design;
using System.Web.UI.WebControls;

namespace System.Design.UI.Design.WebControls;
public abstract class BaseDataBoundControlDesigner : ControlDesigner
{
    [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
    protected BaseDataBoundControlDesigner() { }

    public string DataSource { get; set; }
    public string DataSourceID { get; set; }

    ///TODO: Implement the following methods (Direct Dependency on Windows.Form)
    public static DialogResult ShowCreateDataSourceDialog(ControlDesigner controlDesigner, Type dataSourceType, bool configure, out string dataSourceID) => throw new NotImplementedException();
    public override string GetDesignTimeHtml() => throw new NotImplementedException();
    public override void Initialize(IComponent component) => throw new NotImplementedException();
    protected abstract bool ConnectToDataSource();
    protected abstract void CreateDataSource();
    protected abstract void DataBind(BaseDataBoundControl dataBoundControl);
    protected abstract void DisconnectFromDataSource();
    protected override void Dispose(bool disposing) => throw new NotImplementedException();
    protected override string GetEmptyDesignTimeHtml() => throw new NotImplementedException();
    protected override string GetErrorDesignTimeHtml(Exception e) => throw new NotImplementedException();
    protected virtual void OnDataSourceChanged(bool forceUpdateView) => throw new NotImplementedException();
    protected virtual void OnSchemaRefreshed() => throw new NotImplementedException();
    protected override void PreFilterProperties(IDictionary properties) { throw new NotImplementedException(); }
}
