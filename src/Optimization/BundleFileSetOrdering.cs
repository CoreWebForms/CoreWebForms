// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace System.Web.Optimization
{
    /// <summary>
    /// A named set of files with relative orderings.
    /// </summary>
    public class BundleFileSetOrdering
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        public BundleFileSetOrdering(string name)
        {
            Name = name;
            Files = new List<string>();
        }

        /// <summary>
        /// Identifies the ordered set of files.
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// Ordered list of file name patterns.
        /// </summary>
        /// <remarks>
        /// The file list determines the relative ordering of files within a bundle. Specified patterns allow for one wildcard character (*) 
        /// or substitution token.
        /// </remarks>
        public IList<string> Files
        {
            get;
            private set;
        }
    }
}
