// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SystemWebAdapters;

namespace System.Web;

public static class HandlerExtensions
{
    public static void UseHttpHandlers(this IApplicationBuilder builder)
    {
        builder.UseMiddleware<SetHttpHandlerMiddleware>();
    }

    public static void SetHandler(this HttpContext context, IHttpHandler handler)
    {
        var coreContext = (Microsoft.AspNetCore.Http.HttpContext)context;
        coreContext.Features.GetRequired<IHttpHandlerFeature>().Current = handler;
    }

    private static T GetRequired<T>(this IFeatureCollection features)
    {
        return features.Get<T>() ?? throw new InvalidOperationException();
    }
}
