// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Optimization
{
    /// <summary>
    /// Bundle designed specifically for processing cascading stylesheets (CSS)
    /// </summary>
    public class StyleBundle : Bundle
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StyleBundle"/> class.
        /// </summary>
        /// <param name="virtualPath">The virtual path used to reference the <see cref="StyleBundle"/> from within a view or Web page.</param>
        public StyleBundle(string virtualPath)
            : base(virtualPath, new CssMinify())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StyleBundle"/> class.
        /// </summary>
        /// <param name="virtualPath">The virtual path used to reference the <see cref="StyleBundle"/> from within a view or Web page.</param>
        /// <param name="cdnPath">An alternate url for the bundle when it is stored in a content delivery network.</param>
        public StyleBundle(string virtualPath, string cdnPath)
            : base(virtualPath, cdnPath, new CssMinify())
        {
        }

    }
}
