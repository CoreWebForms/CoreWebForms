// MIT License.

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

