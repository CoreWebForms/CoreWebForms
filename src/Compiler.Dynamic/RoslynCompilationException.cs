// MIT License.

namespace WebForms.Compiler.Dynamic;

internal sealed class RoslynCompilationException : Exception
{
    public RoslynCompilationException(string route, IEnumerable<RoslynError> error)
    {
        Route = route;
        Error = error;
    }

    public string Route { get; }

    public IEnumerable<RoslynError> Error { get; }
}
