// MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SystemWebAdapters.UI.RuntimeCompilation;
using Microsoft.Extensions.FileProviders;

namespace Microsoft.AspNetCore.SystemWebAdapters.UI.DynamicCompilation;

internal sealed class SystemWebCompilation : IPageCompiler
{
    public Task<ICompiledPage> CompilePageAsync(IFileProvider files, string path, CancellationToken token)
    {
        var parser = new PageParser();
        parser.Parse(Array.Empty<string>(), path);

        return Task.FromResult<ICompiledPage>(new SystemWebCompiledPage(path));
    }

    private sealed class SystemWebCompiledPage : ICompiledPage
    {
        public SystemWebCompiledPage(string path)
        {
            AspxFile = path;
        }

        public PathString Path { get; set; }

        public string AspxFile { get; }

        public Type? Type { get; set; }

        public Memory<byte> Error { get; set; }

        public IReadOnlyCollection<string> FileDependencies { get; set; } = Array.Empty<string>();

        public void Dispose()
        {
        }
    }
}
