// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text;
using System.Web.Optimization.Resources;

namespace System.Web.Optimization
{
    /// <summary>
    /// Bundle transformation that performs JavaScript minification.
    /// </summary>
    public class JsMinify : IBundleTransform
    {
        internal static string JsContentType = "text/javascript";

        // Not public since in the future we could add instance data to the transforms
        internal static readonly JsMinify Instance = new JsMinify();

        internal static void GenerateErrorResponse(BundleResponse bundle, IEnumerable<object> errors)
        {
            var errorResponse = new StringBuilder();
            errorResponse.Append("/* ");
            errorResponse.Append(OptimizationResources.MinifyError).Append("\r\n");
            foreach (var error in errors)
            {
                errorResponse.Append(error.ToString()).Append("\r\n");
            }
            errorResponse.Append(" */\r\n");
            errorResponse.Append(bundle.Content);
            bundle.Content = errorResponse.ToString();
        }

        /// <summary>
        /// Transforms the bundle contents by applying javascript minification
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
                // NOTE: Eval immediate treatment is needed for WebUIValidation.js to work properly after minification
                // NOTE: CssMinify does not support important comments, so we are going to strip them in JS minification as well
                string minifiedJs = min.MinifyJavaScript(response.Content, new CodeSettings() { EvalTreatment = EvalTreatment.MakeImmediateSafe, PreserveImportantComments = false });
                if (min.ErrorList.Count > 0) {
                    GenerateErrorResponse(response, min.ErrorList);
                }
                else {
                    response.Content = minifiedJs;
                }
            }
#endif

            response.ContentType = JsContentType;
        }
    }
}
