// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
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

    private sealed class DynamicPageEndpointDataSource : EndpointDataSource
    {
        private readonly ICompiledPagesCollection _collection;
        private CompiledEndpoint _endpoints;

        public DynamicPageEndpointDataSource(ICompiledPagesCollection collection)
        {
            _collection = collection;
            _endpoints = new(Array.Empty<Endpoint>(), Array.Empty<Type>());
        }

        public override IReadOnlyList<Endpoint> Endpoints
        {
            get
            {
                if (ReferenceEquals(_collection.PageTypes, _endpoints.Types))
                {
                    return _endpoints.Endpoints;
                }

                var newList = new List<Endpoint>();
                var types = _collection.PageTypes;

                foreach (var type in types)
                {
                    newList.Add(PageEndpointRouteBuilder.Create(type));
                }

                _endpoints = new(newList, types);

                return newList;
            }
        }

        public override IChangeToken GetChangeToken() => _collection.ChangeToken;

        private readonly record struct CompiledEndpoint(IReadOnlyList<Endpoint> Endpoints, IReadOnlyList<Type> Types);
    }
}
