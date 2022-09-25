// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
