// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace System.Web.Optimization
{
    /// <summary>
    /// Rewrites urls to be absolute so assets will still be found after bundling
    /// </summary>
    public class CssRewriteUrlTransform : IItemTransform
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public CssRewriteUrlTransform()
        {
        }

        internal static string RebaseUrlToAbsolute(string baseUrl, string url)
        {
            // Don't do anything to invalid urls or absolute urls
            if (string.IsNullOrWhiteSpace(url) ||
                string.IsNullOrWhiteSpace(baseUrl) ||
                url.StartsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                return url;
            }

            // NOTE: now we support for ~ app relative urls here
            if (!baseUrl.EndsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                baseUrl += "/";
            }

            return VirtualPathUtility.ToAbsolute(baseUrl + url);
        }

        internal static string ConvertUrlsToAbsolute(string baseUrl, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return content;
            }

            // Replace all urls with absolute urls
            var url = new Regex(@"url\(['""]?(?<url>[^)]+?)['""]?\)");
            return url.Replace(content, ((match) =>
            {
                return "url(" + RebaseUrlToAbsolute(baseUrl, match.Groups["url"].Value) + ")";
            }));
        }

        /// <summary>
        /// Converts any urls in the input to absolute using the base directory of the include virtual path.
        /// </summary>
        /// <param name="includedVirtualPath">The virtual path that was included in the bundle for this item that is being transformed</param>
        /// <param name="input"></param>
        /// <example>
        /// bundle.Include("~/content/some.css") will transform url(images/1.jpg) => url(/content/images/1.jpg)
        /// </example>
        public string Process(string includedVirtualPath, string input)
        {
            if (includedVirtualPath == null)
            {
                throw new ArgumentNullException(nameof(includedVirtualPath));
            }
            // Strip off the ~ that always occurs in app relative virtual paths
            var baseUrl = VirtualPathUtility.GetDirectory(includedVirtualPath.Substring(1));
            return ConvertUrlsToAbsolute(baseUrl, input);
        }
    }
}
