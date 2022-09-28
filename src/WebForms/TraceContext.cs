// MIT License.

namespace System.Web;

public class TraceContext
{
    internal static readonly TraceContext Instance = new();

    private TraceContext()
    {
    }
}
