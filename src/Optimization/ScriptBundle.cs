// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Optimization
{
    /// <summary>
    /// Bundle designed specifically for processing JavaScript
    /// </summary>
    public class ScriptBundle : Bundle
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptBundle"/> class.
        /// </summary>
        /// <param name="virtualPath">The virtual path used to reference the <see cref="ScriptBundle"/> from within a view or Web page.</param>
        public ScriptBundle(string virtualPath) : this(virtualPath, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptBundle"/> class.
        /// </summary>
        /// <param name="virtualPath">The virtual path used to reference the <see cref="ScriptBundle"/> from within a view or Web page.</param>
        /// <param name="cdnPath">An alternate url for the bundle when it is stored in a content delivery network.</param>
        public ScriptBundle(string virtualPath, string cdnPath)
            : base(virtualPath, cdnPath, new JsMinify())
        {
            ConcatenationToken = ";" + Environment.NewLine;
        }
    }
}
