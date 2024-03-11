// MIT License.

using Microsoft.CodeAnalysis;

namespace WebForms.Compiler.Dynamic;

internal sealed class RoslynError
{
    public required string Id { get; init; }

    public required string Message { get; init; }

    public required DiagnosticSeverity Severity { get; init; }

    public required string Location { get; init; }
}
