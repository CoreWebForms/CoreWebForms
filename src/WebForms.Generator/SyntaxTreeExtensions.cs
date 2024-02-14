// MIT License.

using Microsoft.CodeAnalysis;

namespace WebForms.Generator;

internal static class SyntaxTreeExtensions
{
    // Utilize the same logic used by the interceptors API for resolving the source mapped
    // value of a path.
    // https://github.com/dotnet/roslyn/blob/f290437fcc75dad50a38c09e0977cce13a64f5ba/src/Compilers/CSharp/Portable/Compilation/CSharpCompilation.cs#L1063-L1064
    internal static string GetInterceptorFilePath(this SyntaxTree tree, SourceReferenceResolver? resolver) =>
        resolver?.NormalizePath(tree.FilePath, baseFilePath: null) ?? tree.FilePath;
}
