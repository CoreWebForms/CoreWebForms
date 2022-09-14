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

namespace Microsoft.AspNetCore.Builder;

internal class PageEndpointRouteBuilder
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

    public static Endpoint Create(Type type)
    {
        if (!type.IsAssignableTo(typeof(Page)))
        {
            throw new InvalidOperationException($"{type} is not a valid page type");
        }

        if (type.GetCustomAttribute<AspxPageAttribute>() is not { } aspx)
        {
            throw new InvalidOperationException("Page must have a path attribute");
        }

        var path = aspx.Path;

        if (!path.StartsWith('/'))
        {
            path = "/" + path;
        }

        return Create(type, path);
    }

    public static Endpoint Create(Type type, PathString path)
    {
        var pattern = RoutePatternFactory.Parse(path.ToString());
        var builder = new RouteEndpointBuilder(null!, pattern, 0);

        AddPageMetadata(builder, type);
        AddPageRequestDelegate(builder, type);

        return builder.Build();
    }

    private static RouteEndpointBuilder AddPageRequestDelegate(RouteEndpointBuilder builder, Type type)
    {
        builder.RequestDelegate = CreateRequest(type);
        return builder;

        static RequestDelegate CreateRequest(Type type)
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
    }

    private static RouteEndpointBuilder AddPageMetadata(RouteEndpointBuilder builder, Type type)
    {
        foreach (var item in GetMetadataCollection(type))
        {
            builder.Metadata.Add(item);
        }

        return builder;

        static ImmutableList<object> GetMetadataCollection(Type type)
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
    }
}
