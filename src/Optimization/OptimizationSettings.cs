// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Optimization
{
    /// <summary>
    /// Configuration settings used by the <see cref="Optimizer"/> class to generate bundle responses outside of ASP.NET applications.
    /// </summary>
    public class OptimizationSettings
    {

        /// <summary>
        /// The physical file path to resolve the '~' token in virtual paths
        /// </summary>
        public string ApplicationPath { get; set; }

        /// <summary>
        /// The bundle collection to be used
        /// </summary>
        public BundleCollection BundleTable { get; set; }

        /// <summary>
        /// The path to the bundle manifest file that sets up the <see cref="BundleCollection"/>
        /// </summary>
        public string BundleManifestPath { get; set; }

        /// <summary>
        /// Gets or sets a callback function which is invoked after the bundle manifest is loaded to allow further customization of the bundle collection.
        /// </summary>
        public Action<BundleCollection> BundleSetupMethod { get; set; }
    }
}
