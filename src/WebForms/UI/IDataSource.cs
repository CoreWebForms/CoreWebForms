// MIT License.

using System.Collections;

namespace System.Web.UI;

public interface IDataSource
{

    event EventHandler DataSourceChanged;

    DataSourceView GetView(string viewName);

    ICollection GetViewNames();
}

