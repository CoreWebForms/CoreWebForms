// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web.Hosting;

namespace System.Web.Optimization
{
    /// <summary>
    /// Default <see cref="IBundleBuilder"/> which combines files in the bundle.
    /// </summary>
    public class DefaultBundleBuilder : IBundleBuilder
    {
        // We only need one of these since everything it does is currently static
        internal static IBundleBuilder Instance = new DefaultBundleBuilder();

        /// <summary>
        /// The bundle preamble is just a string to string dictionary that we write out at the top of the instrumented output
        /// </summary>
        /// <param name="boundaryValue"></param>
        /// <returns></returns>
        private static Dictionary<string, string> GetInstrumentedBundlePreamble(string boundaryValue)
        {
            var preamble = new Dictionary<string, string>();
            preamble["Bundle"] = "System.Web.Optimization.Bundle";
            preamble["Boundary"] = boundaryValue;
            return preamble;
        }

        /// <summary>
        /// This method is used to generate the boundary header, by default it is simply a hash of the bundle's transform type name
        /// The value only needs to be unique and not conflict with anything within an individual bundle.
        /// </summary>
        /// <param name="bundle"></param>
        /// <returns></returns>
        private static string GetBoundaryIdentifier(Bundle bundle)
        {
            Type transform;
            if (bundle.Transforms != null && bundle.Transforms.Count > 0)
            {
                transform = bundle.Transforms[0].GetType();
            }
            else
            {
                transform = typeof(DefaultTransform);
            }
            // Use the base64 encoded transform type name's hash code
            return Convert.ToBase64String(Encoding.Unicode.GetBytes(transform.FullName.GetHashCode().ToString(CultureInfo.InvariantCulture)));
        }

        /// <summary>
        /// fileHeaderFormat will be expected to take filePath, i.e. "/* a3cf1z2b '{1}'*/"
        /// </summary>
        /// <param name="boundaryValue"></param>
        /// <returns></returns>
        private static string GetInstrumentedFileHeaderFormat(string boundaryValue)
        {
            return "/* " + boundaryValue + " \"{0}\" */";
        }

        internal static string ConvertToAppRelativePath(string appPath, string fullName)
        {
            // Don't need to do anything for base appPath
            if (string.Equals("/", appPath, StringComparison.OrdinalIgnoreCase))
            {
                return fullName;
            }
            string appRelativeFilePath;
            if (!string.IsNullOrEmpty(appPath) && fullName.StartsWith(appPath, StringComparison.OrdinalIgnoreCase))
            {
                appRelativeFilePath = fullName.Replace(appPath, "~/");
            }
            else
            {
                appRelativeFilePath = fullName;
            }
            appRelativeFilePath = appRelativeFilePath.Replace('\\', '/');
            return appRelativeFilePath;
        }

        private static string GetApplicationPath(VirtualPathProvider vpp)
        {
            if (vpp != null && vpp.DirectoryExists("~"))
            {
                var appDir = vpp.GetDirectory("~");
                if (appDir != null)
                {
                    return appDir.VirtualPath;
                }
            }
            return null;
        }

        private static string GetFileHeader(BundleContext context, VirtualFile file, string fileHeaderFormat)
        {
            if (string.IsNullOrEmpty(fileHeaderFormat))
            {
                return string.Empty;
            }
            var appPath = GetApplicationPath(context.VirtualPathProvider);
            return string.Format(CultureInfo.InvariantCulture, fileHeaderFormat, ConvertToAppRelativePath(appPath, file.VirtualPath)) + "\r\n";
        }

        private static string GenerateBundlePreamble(string bundleHash)
        {
            var preambleDictionary = GetInstrumentedBundlePreamble(bundleHash);
            var preamble = new StringBuilder();
            preamble.Append("/* ");
            foreach (var key in preambleDictionary.Keys)
            {
                preamble.Append(key + "=" + preambleDictionary[key] + ";");
            }
            preamble.Append(" */");
            return preamble.ToString();
        }

        /// <summary>
        /// Concatenates the contents of bundle files(after applying any item transforms) to produce the bundle content. 
        /// </summary>
        /// <param name="bundle">The <see cref="Bundle"/> object from which to build the combined content.</param>
        /// <param name="context">The <see cref="BundleContext"/> object that contains state for both the framework configuration and the HTTP request.</param>
        /// <param name="files">The files contained in the bundle.</param>
        /// <returns>The combined content of all files in the bundle.</returns>
        /// <remarks>
        /// Instrumentation mode(for tooling) adds the following to the bundle content
        /// 
        /// 1. A bundle preamble at the start that consists of name/value pairs with a required Boundary token:
        /// /* Bundle=System.Web.Optimization.Bundle;Boundary=MQA2ADkAMgA2ADIANgAwADYANwA=; */
        /// 
        /// 2. A file header that contains the boundary value specified in the preamble, and the virtual file path
        /// /* MQA2ADkAMgA2ADIANgAwADYANwA= "~/mod/modernizr-1.0.js" */
        /// 
        /// 3. Followed by the actual contents of the file
        /// </remarks>
        public string BuildBundleContent(Bundle bundle, BundleContext context, IEnumerable<BundleFile> files)
        {
            if (files == null)
            {
                return string.Empty;
            }
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            if (bundle == null)
            {
                throw new ArgumentNullException(nameof(bundle));
            }

            var bundleBlob = new StringBuilder();
            var bundleHash = "";
            if (context.EnableInstrumentation)
            {
                bundleHash = GetBoundaryIdentifier(bundle);
                bundleBlob.AppendLine(GenerateBundlePreamble(bundleHash));
            }

            string concatToken = null;
            if (!string.IsNullOrEmpty(bundle.ConcatenationToken))
            {
                concatToken = bundle.ConcatenationToken;
            }
            else
            {
                // If JsMinify is used, and no ConcatenationToken is specified, use ';'
                foreach (var transform in bundle.Transforms)
                {
                    if (typeof(JsMinify).IsAssignableFrom(transform.GetType()))
                    {
                        concatToken = ";" + Environment.NewLine;
                        break;
                    }
                }
            }
            if (concatToken == null || context.EnableInstrumentation)
            {
                // If no token specified or we are in instrumentation mode separate using new lines
                concatToken = Environment.NewLine;
            }

            foreach (var file in files)
            {
                if (context.EnableInstrumentation)
                {
                    bundleBlob.Append(GetFileHeader(context, file.VirtualFile, GetInstrumentedFileHeaderFormat(bundleHash)));
                }
                // Apply per file transforms before concatinating
                bundleBlob.Append(file.ApplyTransforms());
                bundleBlob.Append(concatToken);
            }

            return bundleBlob.ToString();
        }
    }
}
