// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.AspNetCore.SystemWebAdapters.Compiler;

internal readonly struct QName
{
    public QName(string ns, string name)
    {
        Namespace = ns;
        Name = name;
    }

    public string Namespace { get; }

    public string Name { get; }
}
