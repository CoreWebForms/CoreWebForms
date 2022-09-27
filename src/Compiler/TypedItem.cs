// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.SystemWebAdapters.Compiler.ParserImpl;

namespace Microsoft.AspNetCore.SystemWebAdapters.Compiler;

internal readonly struct TypedItem
{
    public TypedItem(QName qname, TagAttributes attributes)
    {
        QName = qname;
        Attributes = attributes;
    }

    public QName QName { get; }

    public TagAttributes Attributes { get; }
}
