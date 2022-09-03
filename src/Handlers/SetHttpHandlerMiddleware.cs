// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal class SetHttpHandlerMiddleware
{
    private readonly RequestDelegate _next;

    public SetHttpHandlerMiddleware(RequestDelegate next) => _next = next;

    public Task InvokeAsync(HttpContext context)
    {
        var feature = new HttpHandlerEndpointFeature();

        context.Features.Set<IHttpHandlerFeature>(feature);
        context.Features.Set<IEndpointFeature>(feature);

        return _next(context);
    }
}
