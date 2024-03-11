// MIT License.

#nullable enable

using System.Diagnostics.CodeAnalysis;

namespace WebForms.Features;

public interface IWebFormsCompilationFeature
{
    IReadOnlyCollection<string> Paths { get; }

    bool TryGetException(string path, [MaybeNullWhen(false)] out Exception exception);

    Type? GetForPath(string virtualPath);
}
