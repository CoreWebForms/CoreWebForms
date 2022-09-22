// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Builder;

public static class PageExtensions
{
    public static ISystemWebAdapterBuilder AddWebForms(this ISystemWebAdapterBuilder services)
    {
        services.AddHttpHandlers();
        return services;
    }

    public static void UseWebForms(this IApplicationBuilder app)
    {
        app.Use((ctx, next) =>
        {
            return next(ctx);
        });
    }
}
