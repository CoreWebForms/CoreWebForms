// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.AspNetCore.SystemWebAdapters.Compiler;

internal readonly struct Variable
{
    public Variable(string name, QName qname)
    {
        Name = name;
        Type = qname;
    }

    public string Name { get; }

    public QName Type { get; }
}
