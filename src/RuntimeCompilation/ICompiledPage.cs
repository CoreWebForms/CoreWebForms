// MIT License.

using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.SystemWebAdapters.UI.RuntimeCompilation;

internal interface ICompiledPage : IDisposable
{
    PathString Path { get; }

    string AspxFile { get; }

    Type? Type { get; }

    Memory<byte> Error { get; }

    IReadOnlyCollection<string> FileDependencies { get; }
}
