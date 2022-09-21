// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;
using System.Web;
using System.Web.UI.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
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
        builder.Metadata.Add(new PageEventsFactory(type));

        return builder.Build();
    }
}
