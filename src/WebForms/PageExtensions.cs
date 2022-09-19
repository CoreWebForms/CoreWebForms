// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Web;
using System.Web.UI;
using System.Web.UI.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SystemWebAdapters;

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
            if (ctx.Features.Get<IHttpHandlerFeature>() is { Current: Page page })
            {
                if (ctx.GetEndpoint()?.Metadata.GetMetadata<IPageEvents>() is { } events)
                {
                    page.Features.Set<IPageEvents>(events);
                }

                page.Features.Set<Page>(page);
                page.Features.Set<IUniqueIdGeneratorFeature>(new UniqueIdGeneratorFeature(page));
            }

            return next(ctx);
        });
    }
}
