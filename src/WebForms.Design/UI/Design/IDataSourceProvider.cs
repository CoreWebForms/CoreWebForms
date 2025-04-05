// MIT License.

using System.Collections;

namespace System.Web.UI.Design;

public interface IDataSourceProvider
{
    IEnumerable GetResolvedSelectedDataSource();
    object GetSelectedDataSource();
}
