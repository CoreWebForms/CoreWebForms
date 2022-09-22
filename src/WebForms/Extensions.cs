// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Web.Caching;

namespace System.Web;

internal static class Extensions
{
    public static void Dispose(this CacheDependency dep)
    {
    }

    public static bool IsPost(this HttpRequest request) => string.Equals("POST", request.HttpMethod, StringComparison.OrdinalIgnoreCase);

    public static void InvokeCancellableCallback(this HttpContext context, WaitCallback callback, object state)
        => callback(state);
}
