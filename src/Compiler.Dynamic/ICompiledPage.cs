// MIT License.

using Microsoft.AspNetCore.Http;

namespace WebForms.Compiler.Dynamic;

internal interface ICompiledPage : IDisposable
{
    PathString Path { get; }

    string AspxFile { get; }

    Type? Type { get; }

    Exception? Exception { get; }

    IReadOnlyCollection<string> FileDependencies { get; }
}
