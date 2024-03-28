// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Hosting;

namespace System.Web.Optimization
{
    /// <summary>
    /// Encapsulates the data needed to process a bundle request.
    /// </summary>
    public class BundleContext
    {
        /// <summary>
        /// Gets the Http context for the request.
        /// </summary>
        /// <remarks>
        /// The value for HttpContext will generally be the current instance of <see cref="HttpContext"/>. However, 
        /// using the base wrapper class enables HttpContext to be mocked for unit testing.
        /// </remarks>
        public HttpContextBase HttpContext
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the bundle collection associated with the request.
        /// </summary>
        public BundleCollection BundleCollection
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the virtual path for the bundle request.
        /// </summary>
        public string BundleVirtualPath
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets or sets whether instrumentation mode is enabled.
        /// </summary>
        public bool EnableInstrumentation
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether optimizations are enabled via <see cref="BundleTable.EnableOptimizations"/>
        /// </summary>
        public bool EnableOptimizations
        {
            get;
            set;
        }

        internal VirtualPathProvider VirtualPathProvider
        {
            get;
            set;
        }

#if PORT_CACHING
        private bool? _useServerCache;
        /// <summary>
        /// Gets or sets whether the bundle response should be stored in HttpContext.Cache
        /// </summary>
        public bool UseServerCache
        {
            get
            {
                // Use value if specified
                if (_useServerCache.HasValue)
                {
                    return _useServerCache.Value;
                }
                // Otherwise, only cache if its non null and output cache is the default
                return (HttpContext != null &&
                    HttpContext.Cache != null &&
                    !EnableInstrumentation &&
                    HttpContext.ApplicationInstance != null &&
                    string.Equals(HttpContext.ApplicationInstance.GetOutputCacheProviderName(HttpContext.ApplicationInstance.Context), "AspNetInternalProvider", StringComparison.Ordinal));
            }
            set
            {
                _useServerCache = value;
            }
        }
#endif

        private readonly HashSet<string> _cacheDependencyDirectories = new HashSet<string>();
        internal HashSet<string> CacheDependencyDirectories
        {
            get
            {
                return _cacheDependencyDirectories;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BundleContext"/> class.
        /// </summary>
        /// <param name="context">The Http context for the request.</param>
        /// <param name="collection">The bundle collection associated with the request.</param>
        /// <param name="bundleVirtualPath">The virtual path for the bundle request.</param>
        public BundleContext(HttpContextBase context, BundleCollection collection, string bundleVirtualPath)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }
            if (bundleVirtualPath == null)
            {
                throw new ArgumentNullException(nameof(bundleVirtualPath));
            }
            HttpContext = context;
            BundleCollection = collection;
            BundleVirtualPath = bundleVirtualPath;
            VirtualPathProvider = BundleTable.VirtualPathProvider;
            EnableOptimizations = BundleTable.EnableOptimizations;
        }

        // Unit test constructor
        internal BundleContext()
        {
        }
    }
}
