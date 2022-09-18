// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.AspNetCore.SystemWebAdapters.UI.RuntimeCompilation;

internal interface IPageCompiler
{
    Task<ICompiledPage> CompilePageAsync(PageFile file, CancellationToken token);
}
