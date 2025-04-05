// MIT License.

namespace System.Web.UI.Design;

public interface IDataSourceDesigner
{
    bool CanConfigure { get; }
    bool CanRefreshSchema { get; }

    event EventHandler DataSourceChanged;
    event EventHandler SchemaRefreshed;

    void Configure();
    DesignerDataSourceView GetView(string viewName);
    string[] GetViewNames();
    void RefreshSchema(bool preferSilent);
    void ResumeDataSourceEvents();
    void SuppressDataSourceEvents();
}
