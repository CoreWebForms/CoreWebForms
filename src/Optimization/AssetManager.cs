// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Globalization;
using System.Text;
using System.Web.Optimization.Resources;

namespace System.Web.Optimization
{
    // TODO: do we want to support script/style blocks as well?
    // TODO: do we want to help notice if assets are not rendered?
    /// <summary>
    /// This class will eventually keeps a map of script and css references for the purposes of eliminating duplicate references
    /// and also handling rendering out bundle references correctly based on the EnableOptimizations flag
    /// </summary>
    internal sealed class AssetManager
    {
        internal static readonly object AssetsManagerKey = typeof(AssetManager);

        public AssetManager(HttpContextBase context)
        {
            _httpContext = context;
        }

        private readonly HttpContextBase _httpContext;
        // internal for unit tests
        internal HttpContextBase Context
        {
            get
            {
                return _httpContext;
            }
        }

        // internal for unit tests to hook their own resolve url logic
        private Func<string, string, string> _resolveUrlMethod;
        internal Func<string, string, string> ResolveUrlMethod
        {
            get { return _resolveUrlMethod ?? ((basePath, relativePath) => UrlUtil.Url(basePath, relativePath)); }
            set { _resolveUrlMethod = value; }
        }

        // internal for unit tests to hook its own resolver
        private IBundleResolver _resolver;
        internal IBundleResolver Resolver
        {
            get
            {
                return _resolver ?? BundleResolver.Current;
            }
            set
            {
                _resolver = value;
            }
        }

        // internal for unit tests to hook its own bundle collection
        private BundleCollection _bundles;
        internal BundleCollection Bundles
        {
            get
            {
                return _bundles ?? BundleTable.Bundles;
            }
            set
            {
                _bundles = value;
                _bundles.Context = Context;
            }
        }

        // At runtime we always use BundleTable.EnableOptimizations, but unit tests set this value explicitly
        private bool? _optimizationEnabled;
        internal bool OptimizationEnabled
        {
            get
            {
                if (_optimizationEnabled.HasValue)
                {
                    return _optimizationEnabled.Value;
                }
                return BundleTable.EnableOptimizations;
            }
            set
            {
                _optimizationEnabled = value;
            }
        }

        /// <summary>
        /// Gets or registers the AssetManager for the HttpContext
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static AssetManager GetInstance(HttpContextBase context)
        {
            if (context == null)
            {
                return null;
            }

            var manager = (AssetManager)context.Items[AssetsManagerKey];
            if (manager == null)
            {
                manager = new AssetManager(context);
                context.Items[AssetsManagerKey] = manager;
            }
            return manager;
        }

        // Given a list of asset references, remove duplicate bundle references, and eliminate any explicit references to things inside bundles
        // NOTE: Will not eliminate references that show up twice inside different bundles
        private IEnumerable<AssetTag> EliminateDuplicatesAndResolveUrls(IEnumerable<AssetTag> refs)
        {
            var firstPass = new List<AssetTag>();
            var pathMap = new HashSet<string>();
            var bundledContents = new HashSet<string>();
            var resolver = Resolver;

            // first eliminate any duplicate paths
            foreach (var asset in refs)
            {
                // Leave static assets alone
                if (asset.IsStaticAsset)
                {
                    firstPass.Add(asset);
                    continue;
                }

                var path = asset.Value;
                if (!pathMap.Contains(path))
                {
                    // Need to crack open bundles to look at its contents for the second pass
                    if (resolver.IsBundleVirtualPath(path))
                    {
                        var contents = resolver.GetBundleContents(path);
                        foreach (var filePath in contents)
                        {
                            bundledContents.Add(ResolveVirtualPath(filePath));
                        }
                        // Also need to resolve the bundle url to get the unique version
                        asset.Value = resolver.GetBundleUrl(path);
                        firstPass.Add(asset);
                    }
                    // Non bundles we want to resolve the path and check its not a duplicate before adding
                    else
                    {
                        var resolvedPath = ResolveVirtualPath(path);
                        if (!pathMap.Contains(resolvedPath))
                        {
                            pathMap.Add(resolvedPath);
                            asset.Value = resolvedPath;
                            firstPass.Add(asset);
                        }
                    }

                    pathMap.Add(path);
                }
            }

            // Second pass to eliminate files that are contained inside of bundles
            return firstPass.Where(asset => asset.IsStaticAsset || !bundledContents.Contains(asset.Value));
        }

        /// <summary>
        /// Given a list of asset paths, expands bundles into individual assets if optimizations are off
        /// Returns a deduplicated list of paths to render
        /// </summary>
        /// <param name="assets"></param>
        /// <returns></returns>
        private IEnumerable<AssetTag> DeterminePathsToRender(IEnumerable<string> assets)
        {
            var paths = new List<AssetTag>();
            foreach (var path in assets)
            {
                if (Resolver.IsBundleVirtualPath(path))
                {
                    if (!OptimizationEnabled)
                    {
                        // Get the contents of the bundle
                        var contents = Resolver.GetBundleContents(path);
                        foreach (var filePath in contents)
                        {
                            paths.Add(new AssetTag(filePath));
                        }
                    }
                    else
                    {
                        // Just render the unresolved bundle url(we render the versioned url later)
                        paths.Add(new AssetTag(path));

                        // Look for Cdn fallback if requested
                        if (Bundles.UseCdn)
                        {
                            var bundle = Bundles.GetBundleFor(path);
                            if (bundle != null && !string.IsNullOrEmpty(bundle.CdnPath) && !string.IsNullOrEmpty(bundle.CdnFallbackExpression))
                            {
                                // Note: we know this is a bundle virtual path
                                var script = new AssetTag(string.Format(CultureInfo.InvariantCulture, OptimizationResources.CdnFallBackScriptString, bundle.CdnFallbackExpression, ResolveVirtualPath(path)));
                                script.IsStaticAsset = true;
                                paths.Add(script);
                            }
                        }
                    }
                }
                else
                {
                    paths.Add(new AssetTag(path));
                }
            }

            return EliminateDuplicatesAndResolveUrls(paths);
        }

        // Just explicitly render these paths for styles or scripts
        public IHtmlString RenderExplicit(string tagFormat, params string[] paths)
        {
            var uniqueRefs = DeterminePathsToRender(paths);
            var result = new StringBuilder();
            foreach (var r in uniqueRefs)
            {
                result.Append(r.Render(tagFormat));
                result.Append(Environment.NewLine);
            }
            return new HtmlString(result.ToString());
        }

        /// <summary>
        /// NOTE: Copied from WebPages.TagBuilder
        /// If the path is a valid URI we shouldn't try to resolve it. This is roughly the same logic as HREF.
        /// e.g. if the user puts www.foo.com/myScript.js then we should leave it that way.
        /// e.g. if the user puts ~/foo/myscripts.js we should resolve the ~/ to the correct location.
        /// </summary>
        internal string ResolveVirtualPath(string virtualPath)
        {
            Uri uri;
            if (Uri.TryCreate(virtualPath, UriKind.Absolute, out uri))
            {
                return virtualPath;
            }

            var basePath = "";
            if (_httpContext.Request != null)
            {
                basePath = _httpContext.Request.AppRelativeCurrentExecutionFilePath;
            }
            return ResolveUrlMethod(basePath, virtualPath);
        }

        internal HtmlString ResolveUrl(string url)
        {
            // See if its a bundle first
            if (Resolver.IsBundleVirtualPath(url))
            {
                return new HtmlString(Bundles.ResolveBundleUrl(url));
            }
            // Otherwise just resolve the url
            return new HtmlString(ResolveVirtualPath(url));
        }

        internal sealed class AssetTag
        {
            /// <summary>
            /// This usually is the url to the script/style, but sometimes is the actual script, i.e. cdn fallback script
            /// </summary>
            public string Value { get; set; }

            /// <summary>
            /// True for static assets, i.e. cdn fallback scripts
            /// </summary>
            public bool IsStaticAsset { get; set; }

            public AssetTag(string value)
            {
                Value = value;
            }

            public string Render(string tagFormat)
            {
                if (IsStaticAsset)
                {
                    return Value;
                }
                else
                {
                    return string.Format(CultureInfo.InvariantCulture, tagFormat, HttpUtility.UrlPathEncode(Value));
                }
            }
        }
    }
}
