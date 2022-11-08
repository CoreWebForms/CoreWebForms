// MIT License.

#nullable enable

using System.Diagnostics;
using System.Web.UI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SystemWebAdapters;

namespace Microsoft.AspNetCore.Builder;

public static class PageEndpointConventionExtensions
{
    public static TBuilder WithPageSupport<TBuilder>(this TBuilder builder)
        where TBuilder : IHttpHandlerEndpointConventionBuilder
    {
        builder.Add(builder =>
        {
            var next = builder.RequestDelegate;

            Debug.Assert(next is not null);

            builder.RequestDelegate = PageHandler;

            Task PageHandler(HttpContext context)
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
        });

        return builder;
    }
}
