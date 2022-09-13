// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.Builder;

internal sealed class PageEndpointDataSource : EndpointDataSource, IChangeToken
{
    private readonly List<Endpoint> _endpoints = new();

    public void Add(Type type)
    {
        var endpoint = PageEndpointRouteBuilder.Create(type);

        _endpoints.Add(endpoint);
    }

    public void Add(Type type, PathString path)
    {
        var endpoint = PageEndpointRouteBuilder.Create(type, path);

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
