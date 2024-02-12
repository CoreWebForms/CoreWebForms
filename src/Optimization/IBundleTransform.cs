// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Optimization
{
    /// <summary>
    /// Defines methods for transforming bundle contents.
    /// </summary>
    public interface IBundleTransform
    {
        /// <summary>
        /// Process the bundle contents.
        /// </summary>
        /// <param name="context">The <see cref="BundleContext"/> object that contains state for both the framework configuration and the HTTP request.</param>
        /// <param name="response">A <see cref="BundleResponse"/> object containing the bundle contents.</param>
        void Process(BundleContext context, BundleResponse response);
    }
}
