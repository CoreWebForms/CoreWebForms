// MIT License.

using System;
using System.Diagnostics;

namespace Microsoft.AspNetCore.SystemWebAdapters.Compiler.Syntax;

[DebuggerDisplay("{ToString()}:{GetText()}")]
public struct Location : IEquatable<Location>
{
    public Location(IAspxSource source, int start, int end)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (start < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(start));
        }

        if (start > end)
        {
            throw new ArgumentOutOfRangeException(nameof(end));
        }

        Start = start;
        Length = end - start;
        this.source = source;
    }

    public int Start { get; }

    public int End => Start + Length;

    private readonly IAspxSource source;

    public IAspxSource Source => source ?? AspxSource.Empty;

    public int Length { get; }

    public bool IsEmpty => Length == 0;

    public string GetText() =>
        Source.Text.Substring(Start, Length);

    public static bool operator ==(Location left, Location right) =>
        left.Equals(right);

    public static bool operator !=(Location left, Location right) =>
        !left.Equals(right);

    public bool Equals(Location other) =>
        Start == other.Start &&
        Length == other.Length &&
        Equals(Source, other.Source);

    public override bool Equals(object obj) =>
        obj is Location && Equals((Location)obj);

    public override int GetHashCode() =>
        unchecked(Start * (int)0xA5555529 + Length) ^ Source.GetHashCode();

    public override string ToString() =>
        $"[{Start}..{End})";
}
