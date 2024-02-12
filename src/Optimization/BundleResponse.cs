// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace System.Web.Optimization
{
    /// <summary>
    /// The response data that will be sent in reply to a bundle request.
    /// </summary>
    public class BundleResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Bundle"/> class.
        /// </summary>
        public BundleResponse()
        {
            CreationDate = DateTimeOffset.UtcNow;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Bundle"/> class.
        /// </summary>
        /// <param name="content">The content of the bundle which is sent as the response body.</param>
        /// <param name="files">The list of files that were used to generate the bundle.</param>
        public BundleResponse(string content, IEnumerable<BundleFile> files) : this()
        {
            Content = content;
            Files = files;
            Cacheability = HttpCacheability.Public; // REVIEW: What should the default be, public vs No cache?
        }

        private string _content;
        private string _contentHash;
        /// <summary>
        /// The content of the bundle which is sent as the response body.
        /// </summary>
        public string Content
        {
            get
            {
                return _content;
            }
            set
            {
                _content = value;
                _contentHash = null;
            }
        }

        /// <summary>
        /// The response content-type header.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// The time when this response was created
        /// </summary>
        public DateTimeOffset CreationDate { get; private set; }

        /// <summary>
        /// Enables control over the cache headers that are spent in the bundle response.
        /// </summary>
        public HttpCacheability Cacheability { get; set; }

        /// <summary>
        /// The list of files that were used to generate the bundle.
        /// </summary>
        /// <remarks>
        /// The list of files is preserved in <see cref="BundleResponse"/> so that <see cref="System.Web.Caching.CacheDependency"/> objects can be 
        /// setup to monitor changes to the underlying files and rebuild the bundle when any of those contents change.
        /// </remarks>
        public IEnumerable<BundleFile> Files { get; set; }

        internal static string ComputeHash(string input)
        {
            using (var sha256 = CreateHashAlgorithm())
            {
                var hash = sha256.ComputeHash(Encoding.Unicode.GetBytes(input));
                return HttpServerUtility.UrlTokenEncode(hash);
            }
        }

        /// <summary>
        /// Returns a hashcode of the bundle contents, for purposes of generating a 'versioned' url for cache busting purposes.
        /// This is not used for cryptographic purposes, just as a quick and dirty way to give browsers a different url when the bundle
        /// changes
        /// </summary>
        /// <returns></returns>
        internal string GetContentHashCode()
        {
            if (_contentHash == null)
            {
                if (string.IsNullOrEmpty(Content))
                {
                    _contentHash = string.Empty;
                }
                else
                {
                    _contentHash = ComputeHash(Content);
                }
            }
            return _contentHash;
        }

        private static readonly bool _isMonoRuntime = Type.GetType("Mono.Runtime") != null;
        /// <summary>
        /// Determines if we are to only allow Fips compliant algorithms. 
        /// </summary>
        /// <remarks>
        /// CryptoConfig.AllowOnlyFipsAlgorithms does not exist in Mono. 
        /// </remarks>
        private static bool AllowOnlyFipsAlgorithms
        {
            get
            {
                return !_isMonoRuntime && CryptoConfig.AllowOnlyFipsAlgorithms;
            }
        }

        // Needed to support XP3 which doesn't support SHA256Cng
        private static SHA256 CreateHashAlgorithm()
        {
            if (AllowOnlyFipsAlgorithms)
            {
                throw new InvalidOperationException("Need a fips compliant sha256");
            }
            else
            {
                return SHA256.Create();
            }
        }

    }
}
