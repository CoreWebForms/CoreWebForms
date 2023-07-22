// MIT License.

namespace WebForms.Compiler;

internal sealed class RoslynCompilationException : Exception
{
    public RoslynCompilationException(IEnumerable<RoslynError> error)
    {
        Error = error;
    }

    public IEnumerable<RoslynError> Error { get; }
}
