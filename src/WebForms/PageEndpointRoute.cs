// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Reflection;
using System.Web;
using System.Web.UI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.SystemWebAdapters.UI;

namespace Microsoft.AspNetCore.Builder;

internal class PageEndpointRoute
{
    public static Endpoint? Create(Type type)
    {
        return type.GetCustomAttribute<AspxPageAttribute>() is { Path: { } path } ? Create(type, path) : null;
    }

    public static Endpoint Create(Type type, PathString path)
    {
        var pattern = RoutePatternFactory.Parse(path.ToString());
        var builder = new RouteEndpointBuilder(null!, pattern, 0);

        builder.AddHttpHandler(type);

        var next = builder.RequestDelegate;

        Debug.Assert(next is not null);

        builder.RequestDelegate = context =>
        {
            if (context.Features.Get<IHttpHandlerFeature>() is { Current: Page page })
            {
                page.Features.Set((System.Web.HttpContext)context);
                page.Features.Set(page);
                return next(context);
            }
            else
            {
                context.Response.StatusCode = 500;
                return context.Response.WriteAsync("Invalid page");
            }
        };

        return builder.Build();
    }
}
