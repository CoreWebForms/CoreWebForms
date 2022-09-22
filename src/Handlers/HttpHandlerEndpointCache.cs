// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http;

namespace System.Web;

internal class HttpHandlerEndpointCache
{
    private readonly ConditionalWeakTable<IHttpHandler, Endpoint> _table = new();

    public bool TryGetValue(IHttpHandler handler, [NotNullWhen(true)] out Endpoint? existing)
        => _table.TryGetValue(handler, out existing);

    public void Add(IHttpHandler handler, Endpoint newEndpoint)
        => _table.Add(handler, newEndpoint);
}

