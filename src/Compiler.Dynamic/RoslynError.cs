// MIT License.

using Microsoft.CodeAnalysis;

namespace WebForms.Compiler.Dynamic;

internal sealed class RoslynError
{
    public string Id { get; internal set; } = null!;

    public string Message { get; internal set; } = null!;

    public DiagnosticSeverity Severity { get; internal set; } 

    public string Location { get; internal set; } = null!;
}
