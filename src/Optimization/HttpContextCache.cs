// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Caching;

namespace System.Web.Optimization
{
    /// <summary>
    /// BundleCache that stores the bundle responses in the HttpContext.Cache using the bundle virtual path as the key
    /// </summary>
    internal sealed class HttpContextCache : IBundleCache
    {
        /// <summary>
        /// This cache is only enabled if we have an http context, instrumentation mode is off, and the output cache provider is
        /// the default provider.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool IsEnabled(BundleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            // Enabled only if cache is non null and intrumentation mode is disabled
            return (context.HttpContext != null &&
                context.HttpContext.Cache != null &&
                !context.EnableInstrumentation);
        }

        /// <summary>
        /// Returns the response for the bundle from the cache if it exists, null otherwise
        /// </summary>
        /// <param name="context"></param>
        /// <param name="bundle"></param>
        /// <returns></returns>
        public BundleResponse Get(BundleContext context, Bundle bundle)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return context.HttpContext.Cache[bundle.GetCacheKey(context)] as BundleResponse;
        }

        /// <summary>
        /// Stores the response for the bundle in the cache, also sets up cache depedencies for the virtual files
        /// used for the response
        /// </summary>
        /// <param name="context"></param>
        /// <param name="bundle"></param>
        /// <param name="response"></param>
        public void Put(BundleContext context, Bundle bundle, BundleResponse response)
        {
            var paths = new List<string>();
            paths.AddRange(response.Files.Select(f => f.VirtualFile.VirtualPath));
            paths.AddRange(context.CacheDependencyDirectories);
            var cacheKey = bundle.GetCacheKey(context);
            // REVIEW: Should we store the actual time we read the files?
            var dep = context.VirtualPathProvider.GetCacheDependency(context.BundleVirtualPath, paths, DateTime.UtcNow);
            context.HttpContext.Cache.Insert(cacheKey, response, dep);
            bundle.CacheKeys.Add(cacheKey);
        }
    }
}
