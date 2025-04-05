// MIT License.

namespace System.Web.UI.Design;

public interface IDataBindingSchemaProvider
{
    bool CanRefreshSchema { get; }
    IDataSourceViewSchema Schema { get; }

    void RefreshSchema(bool preferSilent);
}
