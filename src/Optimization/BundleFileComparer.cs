// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace System.Web.Optimization
{
    internal sealed class BundleFileComparer : IEqualityComparer<BundleFile>, IComparer<BundleFile>
    {
        internal static readonly BundleFileComparer Instance = new BundleFileComparer();

        // Should always use the static instance
        private BundleFileComparer()
        {
        }

        public bool Equals(BundleFile x, BundleFile y)
        {
            if (x == null)
            {
                throw new ArgumentNullException(nameof(x)); ;
            }
            if (y == null)
            {
                throw new ArgumentNullException(nameof(y));
            }

            return string.Equals(x.VirtualFile.VirtualPath, y.VirtualFile.VirtualPath, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(BundleFile obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }
            return obj.VirtualFile.VirtualPath.GetHashCode();
        }

        public int Compare(BundleFile x, BundleFile y)
        {
            if (x == null)
            {
                throw new ArgumentNullException(nameof(x)); ;
            }
            if (y == null)
            {
                throw new ArgumentNullException(nameof(y));
            }

            return string.Compare(x.VirtualFile.VirtualPath, y.VirtualFile.VirtualPath, StringComparison.OrdinalIgnoreCase);
        }
    }

}
