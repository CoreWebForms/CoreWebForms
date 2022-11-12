// MIT License.

using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.SystemWebAdapters.UI.RuntimeCompilation;

internal interface ICompiledPage : IDisposable
{
    PathString Path { get; }

    string AspxFile { get; }

    Type? Type { get; }

    Exception? Exception { get; }

    IReadOnlyCollection<string> FileDependencies { get; }
}
