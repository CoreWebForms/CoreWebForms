// MIT License.

namespace System.Web;

internal static class Constant
{
    internal const string CA2301 = "This is for managed types which need to have custom type converters for JSon serialization, I don't think there will be any com interop types for this scenario.";
    internal const string CA1004 = "Generic parameter is preferable to forcing caller to downcast. Has has been approved by API review board. Dev10 701126: Overload added afterall, to allow runtime determination of the type.";
    internal const string CA1720 = "Cannot change parameter name as would break binary compatibility with legacy apps.";
    internal const string CA1859 = "False positive fixed by https://github.com/dotnet/roslyn-analyzers/pull/6421 but not integrated in yet";

}
