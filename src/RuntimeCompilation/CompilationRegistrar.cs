// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.SystemWebAdapters.UI.RuntimeCompilation;

internal sealed class CompilationRegistrar : ICompilationRegistrar
{
    private readonly IQueue _queue;
    private readonly IPageCompiler _compiler;
    private readonly ILoggerFactory _factory;

    public CompilationRegistrar(IPageCompiler compiler, IQueue queue, ILoggerFactory factory)
    {
        _queue = queue;
        _compiler = compiler;
        _factory = factory;
    }

    public ICompiledPagesCollection Register(IFileProvider files)
        => new CompilationCollection(files, _compiler, _queue, _factory);
}
