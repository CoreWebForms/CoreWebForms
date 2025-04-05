// MIT License.

namespace System.Web.UI.Design;

public interface IDataSourceViewSchema
{
    string Name { get; }

    IDataSourceViewSchema[] GetChildren();
    IDataSourceFieldSchema[] GetFields();
}
