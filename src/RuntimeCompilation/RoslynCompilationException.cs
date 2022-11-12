// MIT License.

namespace Microsoft.AspNetCore.SystemWebAdapters.UI.RuntimeCompilation;

internal sealed class RoslynCompilationException : Exception
{
    public RoslynCompilationException(object error)
    {
        Error = error;
    }

    public object Error { get; }
}
