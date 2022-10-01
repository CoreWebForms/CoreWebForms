// MIT License.

namespace System.Web.UI;

public interface IHierarchyData
{

    // properties
    bool HasChildren { get; }

    string Path { get; }

    object Item { get; }

    string Type { get; }

    // methods
    IHierarchicalEnumerable GetChildren();

    IHierarchyData GetParent();
}

