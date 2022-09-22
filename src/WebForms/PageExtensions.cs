// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Web;
using System.Web.UI;
using System.Web.UI.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Builder;

public static class PageExtensions
{
    public static ISystemWebAdapterBuilder AddWebForms(this ISystemWebAdapterBuilder services)
    {
        services.AddHttpHandlers();
        services.Services.AddSingleton<IViewStateSerializer, ViewStateSerializer>();
        return services;
    }

    public static void UseWebForms(this IApplicationBuilder app)
    {
        app.Use((ctx, next) =>
        {
            if (ctx.Features.Get<IHttpHandlerFeature>() is { Current: Page page })
            {
                if (page is IPageEvents events)
                {
                    page.Features.Set<IPageEvents>(events);
                }
                else if (ctx.GetEndpoint()?.Metadata.GetMetadata<IPageEventsFactory>() is { } factory)
                {
                    page.Features.Set<IPageEvents>(factory.Create(page));
                }

                page.Features.Set<Page>(page);
                page.Features.Set<IUniqueIdGeneratorFeature>(new UniqueIdGeneratorFeature(page));
                page.Features.Set<IFormWriterFeature>(new FormWriterFeature(page, page.ClientScript));
                page.Features.Set<IViewStateManager>(new ViewStateManager(page, ctx));
            }

            return next(ctx);
        });
    }
}
