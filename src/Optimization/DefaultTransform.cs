// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Optimization
{
    /// <summary>
    /// Transform which does nothing to the bundle contents. But it helps resolve the content type automatically
    /// </summary>
    internal sealed class DefaultTransform : IBundleTransform
    {
        // Not public since in the future we could add instance data to the transforms
        internal static readonly DefaultTransform Instance = new DefaultTransform();

        /// <summary>
        /// Maps to the content type header of the response.
        /// </summary>
        public string ContentType
        {
            get;
            set;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public DefaultTransform()
        {
        }

        /// <summary>
        /// Constructor which takes the content type.
        /// </summary>
        /// <param name="contentType"></param>
        public DefaultTransform(string contentType)
        {
            ContentType = contentType;
        }

        /// <summary>
        /// Applies no transformation, but does try to infer the content type if not set
        /// using the first file in the response (for js and css files only)
        /// </summary>
        /// <param name="context"></param>
        /// <param name="response"></param>
        public void Process(BundleContext context, BundleResponse response)
        {
            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            // No transform done, just set the content type if specified
            if (!string.IsNullOrEmpty(ContentType))
            {
                response.ContentType = ContentType;
            }
            else
            {
                // Try to infer content type from the bundle files if not specified.
                if (string.IsNullOrEmpty(response.ContentType) && response.Files != null)
                {
                    // only set the content type if the first file has a js or css extension

                    var firstFile = response.Files.FirstOrDefault();
                    if (firstFile != null)
                    {
                        var extension = VirtualPathUtility.GetExtension(firstFile.VirtualFile.VirtualPath);
                        if (string.Equals(extension, ".js", StringComparison.OrdinalIgnoreCase))
                        {
                            response.ContentType = JsMinify.JsContentType;
                        }
                        else if (string.Equals(extension, ".css", StringComparison.OrdinalIgnoreCase))
                        {
                            response.ContentType = CssMinify.CssContentType;
                        }

                    }
                }
            }
        }
    }
}
