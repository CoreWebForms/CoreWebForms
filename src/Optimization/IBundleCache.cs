// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Caching;

namespace System.Web.Optimization
{
    /// <summary>
    /// Defines the methods used for caching a bundle response
    /// </summary>
    internal interface IBundleCache
    {

        /// <summary>
        /// Returns true if this cache is enabled for this context (some like HttpContextCache are not enabled outside of AspNetHosting)
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        bool IsEnabled(BundleContext context);

        /// <summary>
        /// Returns the response for the bundle from the cache if it exists, null otherwise
        /// </summary>
        /// <param name="context"></param>
        /// <param name="bundle"></param>
        /// <returns></returns>
        BundleResponse Get(BundleContext context, Bundle bundle);

        /// <summary>
        /// Stores the response for the bundle in the cache
        /// </summary>
        /// <param name="context"></param>
        /// <param name="bundle"></param>
        /// <param name="response"></param>
        void Put(BundleContext context, Bundle bundle, BundleResponse response);

    }
}
