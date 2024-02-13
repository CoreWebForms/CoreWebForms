// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;
using System.Web.Optimization.Resources;

[assembly: TagPrefix("System.Web.Optimization", "webopt")]

namespace System.Web.Optimization
{
    /// <summary>
    /// Represents a list of file references to be bundled together as a single resource.
    /// </summary>
    /// <remarks>
    /// A bundle is referenced statically via the <see cref="Path"/> property (i.e. Path = ~/mybundle.url).
    /// </remarks>
    public class Bundle
    {
        private IBundleOrderer _orderer;
        private IBundleBuilder _builder;
        private string _path;
        private ItemRegistry _items;
        private List<string> _cacheKeys = new List<string>();
        private bool _enableReplacements = true;
        // REVIEW: adding to the transforms list does not invalidate cache entries since we have no hook
        private IList<IBundleTransform> _transforms = new List<IBundleTransform>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Bundle"/> class.
        /// </summary>
        protected Bundle()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Bundle"/> class.
        /// </summary>
        /// <param name="virtualPath">The virtual path used to reference the <see cref="Bundle"/> from within a view or Web page.</param>
        /// <param name="cdnPath">An alternate url for the bundle when it is stored in a content delivery network.</param>
        /// <param name="transforms">A list of <see cref="IBundleTransform"/> objects which process the contents of the bundle in the order which they are added.</param>
        public Bundle(string virtualPath, string cdnPath, params IBundleTransform[] transforms)
        {
            CdnPath = cdnPath;
            Path = virtualPath;
            if (!virtualPath.StartsWith("~/", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, OptimizationResources.UrlMappings_only_app_relative_url_allowed, virtualPath), nameof(virtualPath));
            }
            if (transforms != null)
            {
                foreach (var transform in transforms)
                {
                    _transforms.Add(transform);
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Bundle"/> class.
        /// </summary>
        /// <param name="virtualPath">The virtual path used to reference the <see cref="Bundle"/> from within a view or Web page.</param>
        /// <param name="transforms">A list of <see cref="IBundleTransform"/> objects which process the contents of the bundle in the order which they are added.</param>
        public Bundle(string virtualPath, params IBundleTransform[] transforms)
            : this(virtualPath, null, transforms)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Bundle"/> class.
        /// </summary>
        /// <param name="virtualPath">The virtual path used to reference the <see cref="Bundle"/> from within a view or Web page.</param>
        public Bundle(string virtualPath)
            : this(virtualPath, (string)null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Bundle"/> class.
        /// </summary>
        /// <param name="virtualPath">The virtual path used to reference the <see cref="Bundle"/> from within a view or Web page.</param>
        /// <param name="cdnPath">An alternate url for the bundle when it is stored in a content delivery network.</param>
        public Bundle(string virtualPath, string cdnPath)
            : this(virtualPath, cdnPath, null)
        {
        }

        /// <summary>
        /// Virtual path used to reference the <see cref="Bundle"/> from within a view or Web page.
        /// </summary>
        public string Path
        {
            get
            {
                return _path;
            }
            protected set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw ExceptionUtil.PropertyNullOrEmpty("Path");
                }
                _path = value;
            }
        }

        /// <summary>
        /// Gets or sets an alternate url for the bundle when it is stored in a content delivery network.
        /// </summary>
        /// <remarks>
        /// In order to configure the Web optimization framework to emit links to the a bundle's CDN url, the <see cref="CdnPath"/> value must not be
        /// null and  <see cref="BundleCollection.UseCdn"/> must equal true.
        /// </remarks>
        public string CdnPath
        {
            get;
            set;
        }

        /// <summary>
        /// Script expression rendered by the <see cref="Scripts"/> helper class to refernce the local bundle file if the CDN is unavailable.
        /// </summary>
        public virtual string CdnFallbackExpression
        {
            get;
            set;
        }

        /// <summary>
        /// Transforms the contents of a bundle
        /// </summary>
        /// <remarks>
        /// By default, the Web optimization framework includes <see cref="IBundleTransform"/> implementations for minifying scripts and styles.
        /// </remarks>
        public IList<IBundleTransform> Transforms
        {
            get
            {
                return _transforms;
            }
        }

        /// <summary>
        /// Collection of BundleItems that represent the contents of the bundle
        /// </summary>
        internal ItemRegistry Items
        {
            get
            {
                if (_items == null)
                {
                    _items = new ItemRegistry();
                }
                return _items;
            }
        }

        /// <summary>
        /// Determines the order of files in a bundle
        /// </summary>
        public virtual IBundleOrderer Orderer
        {
            get
            {
                if (_orderer == null)
                {
                    return DefaultBundleOrderer.Instance;
                }
                return _orderer;
            }
            set
            {
                _orderer = value;
                InvalidateCacheEntries();
            }
        }

        /// <summary>
        /// Builds the bundle content from the individual files included in the <see cref="Bundle"/> object.
        /// </summary>
        public virtual IBundleBuilder Builder
        {
            get
            {
                if (_builder == null)
                {
                    return DefaultBundleBuilder.Instance;
                }
                return _builder;
            }
            set
            {
                _builder = value;
                InvalidateCacheEntries();
            }
        }

        /// <summary>
        /// Specifies whether to use the <see cref="BundleCollection.FileExtensionReplacementList"/>
        /// </summary>
        public virtual bool EnableFileExtensionReplacements
        {
            get
            {
                return _enableReplacements;
            }
            set
            {
                _enableReplacements = value;
                InvalidateCacheEntries();
            }
        }

        /// <summary>
        /// The token inserted between bundled files to ensure that the final bundle content is valid
        /// </summary>
        /// <remarks>
        /// By default, if <see cref="ConcatenationToken"/> is not specified, the Web optimization framework inserts a new line.
        /// </remarks>
        public string ConcatenationToken
        {
            get;
            set;
        }

        /// <summary>
        /// Internal for the default HttpContextCache, consider, make this public when we allow custom caches
        /// </summary>
        internal IList<string> CacheKeys
        {
            get
            {
                return _cacheKeys;
            }
        }

        /// <summary>
        /// Generates an enumeration of <see cref="VirtualFile"/> objects that represent the contents of the bundle.
        /// </summary>
        /// <param name="context">The <see cref="BundleContext"/> object that contains state for both the framework configuration and the HTTP request.</param>
        /// <returns>An enumeration of <see cref="VirtualFile"/> objects that represent the contents of the bundle.</returns>
        public virtual IEnumerable<BundleFile> EnumerateFiles(BundleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var files = new List<BundleFile>();
            foreach (var item in Items)
            {
                item.AddFiles(files, context);
            }
            return files;
        }

        /// <summary>
        /// Apply the set of <see cref="IBundleTransform"/> objects to the bundle content.
        /// </summary>
        /// <param name="context">The <see cref="BundleContext"/> object that contains state for both the framework configuration and the HTTP request.</param>
        /// <param name="bundleContent">The compiled content of all files in the bundle.</param>
        /// <param name="bundleFiles">The list of all file paths in the bundle.</param>
        /// <returns>A <see cref="BundleResponse"/> object containing the processed bundle contents.</returns>
        /// <remarks>
        /// ApplyTransforms iterates through each <see cref="IBundleTransform"/> object that was configured with the bundle when it was created and calls the
        /// transform's <see cref="IBundleTransform.Process"/> method.
        /// </remarks>
        public virtual BundleResponse ApplyTransforms(BundleContext context, string bundleContent, IEnumerable<BundleFile> bundleFiles)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var bundleResponse = new BundleResponse(bundleContent, bundleFiles);
            if (Transforms != null && Transforms.Count > 0)
            {
                foreach (var transform in Transforms)
                {
                    transform.Process(context, bundleResponse);
                }
            }
            else
            {
                // If there are no transforms requested, we still want to do some basic
                // stuff like infer what the proper content type is
                DefaultTransform.Instance.Process(context, bundleResponse);
            }
            return bundleResponse;
        }

        /// <summary>
        /// Processes the bundle request to generate the response.
        /// </summary>
        /// <param name="context">The <see cref="BundleContext"/> object that contains state for both the framework configuration and the HTTP request.</param>
        /// <returns>A <see cref="BundleResponse"/> object containing the processed bundle contents.</returns>
        /// <remarks>
        /// Generating the bundle response is accomplished based on the following steps:
        /// 1. Enumerates the contents of the bundle.
        /// 2. Filters the files using the IgnoreList
        /// 3. Orders the files.
        /// 4. Chooses any replacements using the FileExtensionReplacementList.
        /// 5. Builds the bundle content using the Builder.
        /// 6. Transforms the bundle using the Transform.
        /// </remarks>
        public virtual BundleResponse GenerateBundleResponse(BundleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var bundleFiles = EnumerateFiles(context);
            bundleFiles = context.BundleCollection.IgnoreList.FilterIgnoredFiles(context, bundleFiles);
            bundleFiles = Orderer.OrderFiles(context, bundleFiles);
            if (EnableFileExtensionReplacements)
            {
                bundleFiles = context.BundleCollection.FileExtensionReplacementList.ReplaceFileExtensions(context, bundleFiles);
            }
            var bundleContent = Builder.BuildBundleContent(this, context, bundleFiles);
            return ApplyTransforms(context, bundleContent, bundleFiles);
        }

        /// <summary>
        /// Specifies a set of files to be included in the <see cref="Bundle"/>.
        /// </summary>
        /// <param name="virtualPaths">The virtual path of the file or file pattern to be included in the bundle.</param>
        /// <returns>The <see cref="Bundle"/> object itself for use in subsequent method chaining.</returns>
        /// <remarks>
        /// By default, files are included based on the order in which they are specified in the <paramref name="virtualPaths"/> parameter. This behavior can be overridden
        /// by using the <see cref="Orderer"/> collection, or by creating a custom <see cref="IBundleOrderer"/> object.
        ///
        /// Include also provides limited support for wildcards and substitution tokens in the last path segment. For example:
        /// A prefix wildcard: *js
        /// - or -
        /// A suffix wildcard: jquery*
        /// - or -
        /// The version substitution token: jquery-{version}.js
        /// </remarks>
        public virtual Bundle Include(params string[] virtualPaths)
        {
            var error = Items.Include(virtualPaths);
            if (error != null)
            {
                throw error;
            }
            InvalidateCacheEntries();
            return this;
        }

        /// <summary>
        /// Includes the specified pattern in the <see cref="Bundle"/> with transforms that apply only to this pattern.
        /// </summary>
        /// <param name="virtualPath">The virtual path of the file or file pattern to be included in the bundle.</param>
        /// <param name="transforms">The transforms to apply only to the included items</param>
        /// <returns>The <see cref="Bundle"/> object itself for use in subsequent method chaining.</returns>
        /// <remarks>
        /// Include also provides limited support for wildcards and substitution tokens in the last path segment. For example:
        /// A prefix wildcard: *js
        /// - or -
        /// A suffix wildcard: jquery*
        /// - or -
        /// The version substitution token: jquery-{version}.js
        /// </remarks>
        public virtual Bundle Include(string virtualPath, params IItemTransform[] transforms)
        {
            var error = Items.IncludePath(virtualPath, transforms);
            if (error != null)
            {
                throw error;
            }
            InvalidateCacheEntries();
            return this;
        }

        /// <summary>
        /// Includes all files in a directory that match a search pattern.
        /// </summary>
        /// <param name="directoryVirtualPath">The virtual path to the directory from which to search for files.</param>
        /// <param name="searchPattern">The search pattern to use in selecting files to add to the bundle.</param>
        /// <returns>The <see cref="Bundle"/> object itself for use in subsequent method chaining.</returns>
        public virtual Bundle IncludeDirectory(string directoryVirtualPath, string searchPattern)
        {
            return IncludeDirectory(directoryVirtualPath, searchPattern, searchSubdirectories: false);
        }

        /// <summary>
        /// Includes all files in a directory that match a search pattern.
        /// </summary>
        /// <param name="directoryVirtualPath">The virtual path to the directory from which to search for files.</param>
        /// <param name="searchPattern">The search pattern to use in selecting files to add to the bundle.</param>
        /// <param name="searchSubdirectories">Specifies whether to recursively search subdirectories of <paramref name="directoryVirtualPath"/>.</param>
        /// <returns>The <see cref="Bundle"/> object itself for use in subsequent method chaining.</returns>
        public virtual Bundle IncludeDirectory(string directoryVirtualPath, string searchPattern, bool searchSubdirectories)
        {
            if (ExceptionUtil.IsPureWildcardSearchPattern(searchPattern))
            {
                throw new ArgumentException(OptimizationResources.InvalidWildcardSearchPattern, nameof(searchPattern));
            }
            var patternType = PatternHelper.GetPatternType(searchPattern);
            var error = PatternHelper.ValidatePattern(patternType, searchPattern, "virtualPaths");
            if (error != null)
            {
                throw error;
            }
            error = Items.IncludeDirectory(directoryVirtualPath, searchPattern, patternType, searchSubdirectories);
            if (error != null)
            {
                throw error;
            }

            return this;
        }

        /// <summary>
        /// Intrumentation mode applies only for Page Inspector and will consist of no minification
        /// and a special preamble between files in the bundle.
        /// </summary>
        /// <param name="context">The HTTP context providing details of the request</param>
        /// <returns>A boolean value indicating whether the requestor is Page Inspector.</returns>
        internal static bool GetInstrumentationMode(HttpContextBase context)
        {
            if (context == null || context.Request == null)
            {
                return false;
            }

            var userAgent = context.Request.UserAgent;
            if (!string.IsNullOrEmpty(userAgent) && Regex.IsMatch(userAgent, @"Eureka/(?<version>[\d\.]+)"))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Logic for a bundle request is as follows:
        /// 1. Generate the ordered list of files to include in the bundle i.e. Orderer.OrderFiles(GetFiles())
        /// 2. We read in the contents of the files, generate the BundleResponse, and apply the Transform specified
        /// 4. We send the response using the transformed BundleResponse
        /// </summary>
        /// <param name="context"></param>
        internal void ProcessRequest(BundleContext context)
        {
            context.EnableInstrumentation = GetInstrumentationMode(context.HttpContext);
            var bundleResponse = GetBundleResponse(context);

            // Set to no-cache if the version requested does not match
            var noCache = false;
            var request = context.HttpContext.Request;
            if (request != null)
            {
                var queryVersion = request.QueryString.Get(VersionQueryString);
                if (queryVersion != null && bundleResponse.GetContentHashCode() != queryVersion)
                {
                    noCache = true;
                }
            }

            var ifModifiedSince = request.Headers["If-Modified-Since"];
            // Return 304 not modified only if the bundle response was created after the date
            if (!noCache && !context.EnableInstrumentation && !string.IsNullOrEmpty(ifModifiedSince))
            {
                DateTimeOffset modifiedSinceDate;
                if (DateTimeOffset.TryParse(ifModifiedSince, out modifiedSinceDate) && bundleResponse.CreationDate < modifiedSinceDate)
                {
                    context.HttpContext.Response.StatusCode = 304;
                    return;
                }
            }

            SetHeaders(bundleResponse, context, noCache);
            context.HttpContext.Response.Write(bundleResponse.Content);
        }

        /// <summary>
        /// Uses the cached response or generate the response, internal for BundleResolver to use
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        internal BundleResponse GetBundleResponse(BundleContext context)
        {
            // Check cache first
            var bundleResponse = CacheLookup(context);

            // Cache miss or its an instrumentation request (which we never cache)
            if (bundleResponse == null || context.EnableInstrumentation)
            {
                bundleResponse = GenerateBundleResponse(context);
                UpdateCache(context, bundleResponse);
            }
            return bundleResponse;
        }

        /// <summary>
        /// Returns the full url with content hash if requested for the bundle
        /// </summary>
        /// <param name="context"></param>
        /// <param name="includeContentHash"></param>
        /// <returns></returns>
        internal string GetBundleUrl(BundleContext context, bool includeContentHash = true)
        {
            var bundleVirtualPath = context.BundleVirtualPath;
            if (includeContentHash)
            {
                var bundleResponse = GetBundleResponse(context);
                bundleVirtualPath += "?" + VersionQueryString + "=" + bundleResponse.GetContentHashCode();
            }
            return AssetManager.GetInstance(context.HttpContext).ResolveVirtualPath(bundleVirtualPath);
        }

        private const string VersionQueryString = "v";

        /// <summary>
        /// Used to determine the cache key to store the response for a particular bundle request
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public virtual string GetCacheKey(BundleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            return "System.Web.Optimization.Bundle:" + context.BundleVirtualPath;
        }

        /// <summary>
        /// Returns the first cache hit from the context.BundleCollection.CacheList by default.
        /// Can override to implement custom caching logic.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public virtual BundleResponse CacheLookup(BundleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var cache = context.BundleCollection.Cache;
            if (cache.IsEnabled(context))
            {
                var response = cache.Get(context, this);
                if (response != null)
                {
                    return response;
                }
            }
            return null;
        }

        //
        /// <summary>
        /// Store the response for this bundle instance in the appropriate caches
        /// </summary>
        /// <param name="context"></param>
        /// <param name="response"></param>
        public virtual void UpdateCache(BundleContext context, BundleResponse response)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            var cache = context.BundleCollection.Cache;
            if (cache.IsEnabled(context))
            {
                cache.Put(context, this, response);
            }
        }

        /// <summary>
        /// Invalidates all the cache dependencies for this bundle
        /// </summary>
        internal void InvalidateCacheEntries()
        {
            if (HttpContext.Current != null && HttpContext.Current.Cache != null)
            {
                // Copy the cache deps before clearing so we minimize the window where new bundle requests can be added
                var copy = new List<string>(_cacheKeys);
                _cacheKeys.Clear();
                foreach (var key in copy)
                {
                    HttpContext.Current.Cache.Remove(key);
                }
            }
        }

        private static void SetHeaders(BundleResponse response, BundleContext context, bool noCache)
        {
            if (context.HttpContext.Response != null)
            {
                if (response.ContentType != null)
                {
                    context.HttpContext.Response.ContentType = response.ContentType;
                }

#if PORT_CACHING
                // Do set caching headers if instrumentation mode is on
                if (!context.EnableInstrumentation && context.HttpContext.Response.Cache != null)
                {
                    // NOTE: These caching headers were based off of what AssemblyResourceLoader does
                    // TODO: let the bundle change the caching semantics?
                    HttpCachePolicyBase cachePolicy = context.HttpContext.Response.Cache;
                    if (noCache)
                    {
                        cachePolicy.SetCacheability(HttpCacheability.NoCache);
                    }
                    else
                    {
                        cachePolicy.SetCacheability(response.Cacheability);
                        cachePolicy.SetOmitVaryStar(true);
                        cachePolicy.SetExpires(DateTime.Now.AddYears(1));
                        cachePolicy.SetValidUntilExpires(true);
                        cachePolicy.SetLastModified(DateTime.Now);
                        cachePolicy.VaryByHeaders["User-Agent"] = true;
                        // CONSIDER: Last modified logic, need to compute a hash of all the dates/ETag support
                    }
                }
#endif
            }
        }
    }
}
