// MIT License.

using System;
using System.CodeDom.Compiler;

namespace Microsoft.AspNetCore.SystemWebAdapters.Compiler;

internal sealed class IndentClose : IDisposable
{
    private readonly bool _includeBrace;
    private readonly IndentedTextWriter _writer;

    public IndentClose(IndentedTextWriter writer, bool includeBrace)
    {
        _includeBrace = includeBrace;
        _writer = writer;
    }

    public void Dispose()
    {
        _writer.Indent--;

        if (_includeBrace)
        {
            _writer.WriteLine("}");
        }
    }
}
