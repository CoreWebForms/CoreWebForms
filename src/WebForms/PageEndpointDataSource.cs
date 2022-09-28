// MIT License.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.Builder;

internal sealed class PageEndpointDataSource : EndpointDataSource, IChangeToken
{
    private readonly List<Endpoint> _endpoints = new();

    public void Add(Type type) => Add(type, true);

    internal void Add(Type type, bool requireAttribute)
    {
        var endpoint = PageEndpointRoute.Create(type);

        if (endpoint is null)
        {
            if (requireAttribute)
            {
                throw new InvalidOperationException("Page must be annotated with AspPageAttribute if path is not specified");
            }
        }
        else
        {
            _endpoints.Add(endpoint);
        }
    }

    public void Add(Type type, PathString path)
    {
        var endpoint = PageEndpointRoute.Create(type, path);

        _endpoints.Add(endpoint);
    }

    public override IReadOnlyList<Endpoint> Endpoints => _endpoints;

    bool IChangeToken.ActiveChangeCallbacks => false;

    bool IChangeToken.HasChanged => false;

    public override IChangeToken GetChangeToken() => this;

    IDisposable IChangeToken.RegisterChangeCallback(Action<object> callback, object state)
        => EmptyDisposable.Instance;

    private sealed class EmptyDisposable : IDisposable
    {
        public static IDisposable Instance { get; } = new EmptyDisposable();

        public void Dispose()
        {
        }
    }
}
