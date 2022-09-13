// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.SystemWebAdapters.UI.RuntimeCompilation;

namespace Microsoft.Extensions.DependencyInjection;

public static class DynamicPagesServices
{
    public static void AddDynamicPages(this ISystemWebAdapterBuilder services)
    {
        services.Services.AddTransient<IPageCompiler, RoslynPageCompiler>();
        services.Services.AddSingleton<ICompilationRegistrar, CompilationRegistrar>();
    }
}
