// MIT License.

namespace Microsoft.AspNetCore.SystemWebAdapters.UI.RuntimeCompilation;

internal sealed class RoslynCompilationException : Exception
{
    public RoslynCompilationException(IEnumerable<RoslynError> error)
    {
        Error = error;
    }

    public IEnumerable<RoslynError> Error { get; }
}
