// MIT License.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.FileProviders;
using WebForms.Features;

namespace WebForms.Compiler.Dynamic;

public interface IWebFormsCompiler
{
    IFileProvider Files { get; }

    ICompilationResult CompilePages(ICompilationStrategy outputProvider, CancellationToken token);
}

public interface ICompilationStrategy
{
    bool HandleExceptions { get; }

    Stream CreatePeStream(string route, string typeName);

    Stream CreatePdbStream(string route, string typeName);

    bool HandleErrors(string route, ImmutableArray<Diagnostic> errors);
}

public interface ICompilationResult : IDisposable
{
    IWebFormsCompilationFeature Types { get; }
}
