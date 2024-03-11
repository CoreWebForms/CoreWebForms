// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Optimization
{
    /// <summary>
    /// Defines methods for ordering files within a <see cref="Bundle"/>.
    /// </summary>
    public interface IBundleOrderer
    {
        /// <summary>
        /// Orders the files within a bundle.
        /// </summary>
        /// <param name="context">The <see cref="BundleContext"/> object that contains state for both the framework configuration and the HTTP request.</param>
        /// <param name="files">The files contained in the bundle.</param>
        /// <returns>An ordered enumeration of <see cref="VirtualFile"/> objects.</returns>
        IEnumerable<BundleFile> OrderFiles(BundleContext context, IEnumerable<BundleFile> files);
    }
}
