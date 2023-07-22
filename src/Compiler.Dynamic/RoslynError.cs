// MIT License.

namespace WebForms.Compiler.Dynamic;

internal sealed class RoslynError
{
    public string Id { get; internal set; } = null!;

    public string Message { get; internal set; } = null!;

    public string Severity { get; internal set; } = null!;

    public string Location { get; internal set; } = null!;
}
