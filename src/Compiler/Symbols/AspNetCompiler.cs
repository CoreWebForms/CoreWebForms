// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using System.Collections.Generic;

namespace Microsoft.AspNetCore.SystemWebAdapters.Compiler.Symbols;

public class AspNetCompiler
{
    public static ParsedPage ParsePage(string path, string contents, IEnumerable<ControlInfo> controlInfo)
        => SymbolCreator.ParsePage(path, contents, controlInfo);
}

