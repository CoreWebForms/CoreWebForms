// MIT License.

using System.Collections;

namespace System.Web.UI;
public delegate void DataSourceViewSelectCallback(IEnumerable data);

// returns whether the exception was handled
public delegate bool DataSourceViewOperationCallback(int affectedRecords, Exception ex);
