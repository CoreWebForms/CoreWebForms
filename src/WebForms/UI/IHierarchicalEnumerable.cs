// MIT License.

using System.Collections;

namespace System.Web.UI;
public interface IHierarchicalEnumerable : IEnumerable
{

    IHierarchyData GetHierarchyData(object enumeratedItem);
}
