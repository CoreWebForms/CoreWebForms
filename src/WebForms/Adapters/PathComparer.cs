// MIT License.

#nullable enable

using System.Diagnostics.CodeAnalysis;

namespace WebForms.Internal;

/// <summary>
/// A string comparer comparer that ignores any '~' or '/' at the beginning.
/// </summary>
internal sealed class PathComparer : IEqualityComparer<string>
{
    private PathComparer()
    {
    }

    public static IEqualityComparer<string> Instance { get; } = new PathComparer();

    public bool Equals(string? x, string? y)
    {
        if (x is null && y is null)
        {
            return true;
        }

        if (x is null || y is null)
        {
            return false;
        }

        return Normalized(x).Equals(Normalized(y), StringComparison.OrdinalIgnoreCase);
    }

    public int GetHashCode([DisallowNull] string obj)
        => string.GetHashCode(Normalized(obj), StringComparison.OrdinalIgnoreCase);

    private static ReadOnlySpan<char> Normalized(string s) => s.AsSpan().TrimStart("~/");
}
