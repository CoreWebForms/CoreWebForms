// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace System.Web.Optimization
{
    /// <summary>
    /// Defines the methods used for building a bundle's contents from individual files
    /// </summary>
    public interface IBundleBuilder
    {
        /// <summary>
        /// Concatenates files inside the bundle.
        /// </summary>
        /// <param name="bundle">The <see cref="Bundle"/> object from which to build the combined content.</param>
        /// <param name="context">The <see cref="BundleContext"/> object that contains state for both the framework configuration and the HTTP request.</param>
        /// <param name="files">The files contained in the bundle.</param>
        /// <returns></returns>
        /// <remarks>The string that is returned is used to initially populate the value of <see cref="BundleResponse.Content"/>.</remarks>
        string BuildBundleContent(Bundle bundle, BundleContext context, IEnumerable<BundleFile> files);
    }
}
