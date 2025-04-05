// MIT License.

using System.Collections;
using System.ComponentModel.Design;
using System.Design.UI.Design.WebControls;
using System.Runtime;
using System.Web.UI.WebControls;

namespace System.Web.UI.Design.WebControls;

public class DataBoundControlDesigner : BaseDataBoundControlDesigner, IDataBindingSchemaProvider, IDataSourceProvider
{
    [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
    public DataBoundControlDesigner() { }

    public override DesignerActionListCollection ActionLists { get; }
    public string DataMember { get; set; }
    public IDataSourceDesigner DataSourceDesigner { get; }
    public DesignerDataSourceView DesignerView { get; }

    public bool CanRefreshSchema => throw new NotImplementedException();

    public IDataSourceViewSchema Schema => throw new NotImplementedException();

    protected virtual int SampleRowCount { get; }
    protected virtual bool UseDataSourcePickerActionList { get; }

    public IEnumerable GetResolvedSelectedDataSource()
    {
        throw new NotImplementedException();
    }

    public object GetSelectedDataSource()
    {
        throw new NotImplementedException();
    }

    public void RefreshSchema(bool preferSilent)
    {
        throw new NotImplementedException();
    }

    protected override bool ConnectToDataSource() => throw new NotImplementedException();
    protected override void CreateDataSource() => throw new NotImplementedException();
    protected override void DataBind(BaseDataBoundControl dataBoundControl) => throw new NotImplementedException();
    protected override void DisconnectFromDataSource() => throw new NotImplementedException();
    protected override void Dispose(bool disposing) => throw new NotImplementedException();
    protected virtual IEnumerable GetDesignTimeDataSource() => throw new NotImplementedException();
    protected virtual IEnumerable GetSampleDataSource() => throw new NotImplementedException();
    protected override void PreFilterProperties(IDictionary properties) => throw new NotImplementedException();
}
