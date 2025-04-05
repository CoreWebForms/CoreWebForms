// MIT License.

namespace System.Web.UI.Design;

public interface IDataSourceFieldSchema
{
    Type DataType { get; }
    bool Identity { get; }
    bool IsReadOnly { get; }
    bool IsUnique { get; }
    int Length { get; }
    string Name { get; }
    bool Nullable { get; }
    int Precision { get; }
    bool PrimaryKey { get; }
    int Scale { get; }
}
