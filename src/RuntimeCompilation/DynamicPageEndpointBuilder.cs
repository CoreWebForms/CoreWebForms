// MIT License.

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
    public static IEndpointConventionBuilder MapDynamicAspxPages(this IEndpointRouteBuilder endpoints, IFileProvider files)
    {
        var registrar = endpoints.ServiceProvider.GetRequiredService<ICompilationRegistrar>();
        var collection = registrar.Register(files);

        var source = new DynamicPageEndpointDataSource(collection);
        endpoints.DataSources.Add(source);

        return source;
    }

    private sealed class DynamicPageEndpointDataSource : EndpointDataSource, IDisposable, IEndpointConventionBuilder
    {
        private readonly ICompiledPagesCollection _collection;
        private readonly List<Action<EndpointBuilder>> _conventions = new();

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
                        var endpoint = PageEndpointRoute.Create(type, page.Path);
                        ApplyConventions(endpoint);
                        newList.Add(endpoint.Build());
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

        private void ApplyConventions(EndpointBuilder builder)
        {
            foreach (var c in _conventions)
            {
                c(builder);
            }
        }

        void IEndpointConventionBuilder.Add(Action<EndpointBuilder> convention)
            => _conventions.Add(convention);

        private readonly record struct CompiledEndpoint(IReadOnlyList<Endpoint> Endpoints, IReadOnlyList<ICompiledPage> Types);
    }
}
