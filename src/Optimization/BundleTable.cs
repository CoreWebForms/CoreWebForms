// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Hosting;
using Microsoft.Extensions.DependencyInjection;
using WebForms;
namespace System.Web.Optimization
{
    /// <summary>
    /// Static holder class for the default bundle collection
    /// </summary>
    public static class BundleTable
    {
        private static BundleCollection _instance = new BundleCollection();

        /// <summary>
        /// Gets the default bundle collection.
        /// </summary>
        public static BundleCollection Bundles
        {
            get
            {
                EnsureBundleSetup();
                return _instance;
            }
        }

        private static bool _enableOptimizations = true;
        private static bool _enableOptimizationsSet;
        /// <summary>
        /// Gets or sets whether bundling and minification is enabled.
        /// </summary>
        public static bool EnableOptimizations
        {
            get
            {
                if (!_enableOptimizationsSet && HttpContext.Current != null)
                {
                    return !HttpContext.Current.IsDebuggingEnabled;
                }
                return _enableOptimizations;
            }
            set
            {
                _enableOptimizations = value;
                _enableOptimizationsSet = true;
            }
        }

        private static VirtualPathProvider _vpp;
        /// <summary>
        /// Gets or sets the <see cref="VirtualPathProvider"/> to be used in resolving bundle files.
        /// </summary>
        /// <remarks>
        /// If a custom <see cref="VirtualPathProvider"/> is set, it will be used instead of the default <see cref="HostingEnvironment.VirtualPathProvider"/>
        /// </remarks>
        public static VirtualPathProvider VirtualPathProvider
        {
            get
            {
                return _vpp ?? HttpRuntimeHelper.Services.GetRequiredService<VirtualPathProvider>();
            }
            set
            {
                _vpp = value;
            }
        }

        private static bool _readBundleManifest;
        private static void EnsureBundleSetup()
        {
            // This should not cause any issues if there's a race condition here, because duplicate adds will just be effective no-ops
            if (!_readBundleManifest)
            {
                _readBundleManifest = true;
                var bundleManifest = BundleManifest.ReadBundleManifest();
                if (bundleManifest != null)
                {
                    bundleManifest.Register(BundleTable.Bundles);
                }
            }
        }

    }
}
