// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.AspNetCore.SystemWebAdapters.UI.RuntimeCompilation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.Builder;

public static class DynamicPageEndpointBuilder
{
    public static void MapDynamicAspxPages(this IEndpointRouteBuilder endpoints, IFileProvider files)
    {
        var registrar = endpoints.ServiceProvider.GetRequiredService<ICompilationRegistrar>();
        var collection = registrar.Register(files);

        endpoints.DataSources.Add(new DynamicPageEndpointDataSource(collection));
    }

    private sealed class DynamicPageEndpointDataSource : EndpointDataSource, IDisposable
    {
        private readonly ICompiledPagesCollection _collection;
        private CompiledEndpoint _endpoints;

        public DynamicPageEndpointDataSource(ICompiledPagesCollection collection)
        {
            _collection = collection;
            _endpoints = new(Array.Empty<Endpoint>(), Array.Empty<ICompiledPage>());
        }

        public override IReadOnlyList<Endpoint> Endpoints
        {
            get
            {
                if (ReferenceEquals(_collection.Pages, _endpoints.Types))
                {
                    return _endpoints.Endpoints;
                }

                var newList = new List<Endpoint>();
                var pages = _collection.Pages;

                foreach (var page in pages)
                {
                    if (page.Type is { } type)
                    {
                        newList.Add(PageEndpointRoute.Create(type, page.Path));
                    }
                    else
                    {
                        var pattern = RoutePatternFactory.Parse(page.Path);
                        var endpoint = new RouteEndpoint(ctx => ctx.Response.Body.WriteAsync(page.Error).AsTask(), pattern, 0, null, null);
                        newList.Add(endpoint);
                    }
                }

                _endpoints = new(newList, pages);

                return newList;
            }
        }

        void IDisposable.Dispose() => _collection.Dispose();

        public override IChangeToken GetChangeToken() => _collection.ChangeToken;

        private readonly record struct CompiledEndpoint(IReadOnlyList<Endpoint> Endpoints, IReadOnlyList<ICompiledPage> Types);
    }
}
