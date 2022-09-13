// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Immutable;
using System.Reflection;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.SystemWebAdapters.UI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.Builder;

public static class PageEndpointBuilder
{
    private static readonly ImmutableList<object> _metadata = new object[]
    {
        new BufferResponseStreamAttribute(),
        new PreBufferRequestStreamAttribute(),
        new SetThreadCurrentPrincipalAttribute(),
        new SingleThreadedRequestAttribute(),
    }.ToImmutableList();

    private static readonly ImmutableList<object> _metadataReadonlySession = _metadata.Add(new SessionAttribute { IsReadOnly = true });
    private static readonly ImmutableList<object> _metadataSession = _metadata.Add(new SessionAttribute { IsReadOnly = false });

    public static void MapAspxPages(this IEndpointRouteBuilder builder, params Assembly[] assemblies)
    {
        if (assemblies.Length == 0)
        {
            if (Assembly.GetEntryAssembly() is { } entry)
            {
                assemblies = new[] { entry };
            }
            else
            {
                return;
            }
        }

        var dataSource = builder.GetPageDataSource();

        foreach (var assembly in assemblies)
        {
            foreach (var type in assembly.GetExportedTypes())
            {
                if (type.IsAssignableTo(typeof(Page)) && type.GetCustomAttribute<AspxPageAttribute>() is { } aspx)
                {
                    dataSource.Add(type, aspx.Path);
                }
            }
        }
    }

    public static void MapAspxPage<TPage>(this IEndpointRouteBuilder endpoints, PathString path)
        where TPage : Page
        => endpoints.GetPageDataSource().Add(typeof(TPage), path);

    private static ImmutableList<object> GetMetadataCollection(Type type)
    {
        if (type.IsAssignableTo(typeof(IReadOnlySessionState)))
        {
            return _metadataReadonlySession;
        }

        if (type.IsAssignableTo(typeof(IRequiresSessionState)))
        {
            return _metadataSession;
        }

        return _metadata;
    }

    private static PageEndpointDataSource GetPageDataSource(this IEndpointRouteBuilder endpoints)
    {
        var dataSource = endpoints.DataSources.OfType<PageEndpointDataSource>().FirstOrDefault();

        if (dataSource is null)
        {
            dataSource = new PageEndpointDataSource();
            endpoints.DataSources.Add(dataSource);
        }

        return dataSource;
    }

    private sealed class PageEndpointDataSource : EndpointDataSource, IChangeToken, IDisposable
    {
        private readonly List<Endpoint> _endpoints = new();

        public void Add(Type type, PathString path)
        {
            var pattern = RoutePatternFactory.Parse(path.ToString());
            var builder = new RouteEndpointBuilder(CreateRequest(type), pattern, 0);

            foreach (var item in GetMetadataCollection(type))
            {
                builder.Metadata.Add(item);
            }

            _endpoints.Add(builder.Build());
        }

        private static RequestDelegate CreateRequest(Type type)
        {
            var factory = ActivatorUtilities.CreateFactory(type, Array.Empty<Type>());
            var pageEvents = new PageEvents(type);

            return Create;

            Task Create(HttpContextCore context)
            {
                var page = (Page)factory(context.RequestServices, null);

                page.Features.Set<IPageEvents>(pageEvents);
                page.Features.Set<Page>(page);
                page.Features.Set<IUniqueIdGeneratorFeature>(new UniqueIdGeneratorFeature(page));

                return page.ProcessAsync(context);
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
