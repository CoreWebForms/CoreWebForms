// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Optimization
{
    /// <summary>
    /// Bundle transformation that performs CSS minification.
    /// </summary>
    public class CssMinify : IBundleTransform
    {
        // Not public since in the future we could add instance data to the transforms
        internal static readonly CssMinify Instance = new CssMinify();

        internal static string CssContentType = "text/css";

        /// <summary>
        /// Minifies the supplied CSS bundle and sets the Http content-type header to 'text/css'
        /// </summary>
        /// <param name="context">The <see cref="BundleContext"/> object that contains state for both the framework configuration and the HTTP request.</param>
        /// <param name="response">A <see cref="BundleResponse"/> object containing the bundle contents.</param>
        public virtual void Process(BundleContext context, BundleResponse response)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

#if PORT_MINIFY
            // Don't minify in Instrumentation mode
            if (!context.EnableInstrumentation) {
                Minifier min = new Minifier();
                string minifiedCss = min.MinifyStyleSheet(response.Content, new CssSettings() { CommentMode = CssComment.None });
                if (min.ErrorList.Count > 0) {
                    JsMinify.GenerateErrorResponse(response, min.ErrorList);
                }
                else {
                    response.Content = minifiedCss;
                }
            }
#endif

            response.ContentType = CssContentType;
        }
    }
}
