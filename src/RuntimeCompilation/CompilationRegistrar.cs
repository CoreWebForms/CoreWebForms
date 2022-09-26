// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.FileProviders;

namespace Microsoft.AspNetCore.SystemWebAdapters.UI.RuntimeCompilation;

internal sealed class CompilationRegistrar : ICompilationRegistrar
{
    private readonly IQueue _queue;
    private readonly IPageCompiler _compiler;

    public CompilationRegistrar(IPageCompiler compiler, IQueue queue)
    {
        _queue = queue;
        _compiler = compiler;
    }

    public ICompiledPagesCollection Register(IFileProvider files)
        => new CompilationCollection(files, _compiler, _queue );
}
