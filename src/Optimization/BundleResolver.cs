// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Hosting;

namespace System.Web.Optimization
{
    /// <summary>
    /// Used by the scripts and styles helper classes as well as System.Web.Extension.ScriptManager 
    /// to determine what urls are bundles and extract the contents of the bundle when
    /// <see cref="BundleTable.EnableOptimizations"/> is false.
    /// </summary>
    public class BundleResolver : IBundleResolver
    {

        private static BundleResolver _default = new BundleResolver();
        private static IBundleResolver _current;
        /// <summary>
        /// ScriptManager uses reflection against System.Web.Optimization.BundleResolver.Current
        /// </summary>
        public static IBundleResolver Current
        {
            get
            {
                return _current ?? _default;
            }
            set
            {
                _current = value;
            }
        }

        private BundleCollection Bundles { get; set; }

        private HttpContextBase _context;
        internal HttpContextBase Context
        {
            get
            {
                return _context ?? new HttpContextWrapper(HttpContext.Current);
            }
            set
            {
                _context = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BundleResolver"/> class.
        /// </summary>
        /// <remarks>Without an explicit <see cref="BundleCollection"/>, the resolver will be initialized with <see cref="BundleTable.Bundles"/></remarks>
        public BundleResolver() : this(BundleTable.Bundles)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BundleResolver"/> class.
        /// </summary>
        /// <param name="bundles">The <see cref="BundleCollection"/> to use in resolving requests.</param>
        public BundleResolver(BundleCollection bundles) : this(bundles, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BundleResolver"/> class.
        /// </summary>
        /// <param name="bundles">The <see cref="BundleCollection"/> to use in resolving requests.</param>
        /// <param name="context">The Http context for the request.</param>
        public BundleResolver(BundleCollection bundles, HttpContextBase context)
        {
            Bundles = bundles;
            Context = context;
        }

        /// <summary>
        /// Determines if the virtual path corresponds to a bundle.
        /// </summary>
        /// <param name="virtualPath">The virtual path requested.</param>
        /// <returns>A boolean value indicating whether the virtual path corresponds to a bundle.</returns>
        public bool IsBundleVirtualPath(string virtualPath)
        {
            if (ExceptionUtil.ValidateVirtualPath(virtualPath, "virtualPath") != null)
            {
                return false;
            }
            return (Bundles.GetBundleFor(virtualPath) != null);
        }

        /// <summary>
        /// Gets a set of file paths that correspond to the contents of a bundle. 
        /// </summary>
        /// <param name="virtualPath">The virtual path requested.</param>
        /// <returns>An enumeration of application-relative virtual paths to the contents of a bundle.</returns>
        public IEnumerable<string> GetBundleContents(string virtualPath)
        {
            if (ExceptionUtil.ValidateVirtualPath(virtualPath, "virtualPath") != null)
            {
                return null;
            }
            var bundle = Bundles.GetBundleFor(virtualPath);
            if (bundle == null)
            {
                return null;
            }

            var bundleContents = new List<string>();
            var context = new BundleContext(Context, Bundles, virtualPath);
            var response = bundle.GetBundleResponse(context);
            foreach (var file in response.Files)
            {
                bundleContents.Add(file.IncludedVirtualPath);
            }

            return bundleContents;
        }

        /// <summary>
        /// Gets the url for a bundle.
        /// </summary>
        /// <param name="virtualPath">The virtual path requested.</param>
        /// <returns>The versioned bundle url or the unmodified virtual path if it does not correspond to a bundle.</returns>
        public string GetBundleUrl(string virtualPath)
        {
            if (ExceptionUtil.ValidateVirtualPath(virtualPath, "virtualPath") != null)
            {
                return null;
            }
            return Bundles.ResolveBundleUrl(virtualPath);
        }
    }
}
