// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Web;
using System.Web.UI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.Builder;

public static class PageEndpointBuilder
{
    public static void MapPage<TPage>(this IEndpointRouteBuilder endpoints, PathString path)
        where TPage : Page
    {
        var dataSource = endpoints.DataSources.OfType<PageEndpointDataSource>().FirstOrDefault();

        if (dataSource is null)
        {
            dataSource = new PageEndpointDataSource();
            endpoints.DataSources.Add(dataSource);
        }

        dataSource.Add<TPage>(path);
    }

    private sealed class PageEndpointDataSource : EndpointDataSource, IChangeToken, IDisposable
    {
        private readonly List<Endpoint> _endpoints = new();

        public void Add<TPage>(PathString path)
            where TPage : Page
        {
            var pattern = RoutePatternFactory.Parse(path.ToString());
            var builder = new RouteEndpointBuilder(CreateRequest<TPage>(), pattern, 0);
            _endpoints.Add(builder.Build());
        }

        private static RequestDelegate CreateRequest<TPage>()
            where TPage : Page
        {
            var factory = ActivatorUtilities.CreateFactory(typeof(TPage), Array.Empty<Type>());

            return Create;

            Task Create(Microsoft.AspNetCore.Http.HttpContext context)
            {
                var page = (IHttpAsyncHandler)factory(context.RequestServices, null);

                return Task.Factory.FromAsync((cb, state) => page.BeginProcessRequest(context, cb, state), page.EndProcessRequest, null);
            }
        }

        public override IReadOnlyList<Endpoint> Endpoints => _endpoints;

        bool IChangeToken.ActiveChangeCallbacks => false;

        bool IChangeToken.HasChanged => false;

        public override IChangeToken GetChangeToken() => this;

        IDisposable IChangeToken.RegisterChangeCallback(Action<object> callback, object state)
            => this;

        void IDisposable.Dispose()
        {
        }
    }
}
