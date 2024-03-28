// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Hosting;

namespace System.Web.Optimization
{
    internal sealed class VirtualFileComparer : IEqualityComparer<VirtualFile>, IComparer<VirtualFile>
    {
        internal static readonly VirtualFileComparer Instance = new VirtualFileComparer();

        // Should always use the static instance
        private VirtualFileComparer()
        {
        }

        public bool Equals(VirtualFile x, VirtualFile y)
        {
            if (x == null)
            {
                throw new ArgumentNullException(nameof(x));
            }
            if (y == null)
            {
                throw new ArgumentNullException(nameof(y));
            }

            return string.Equals(x.VirtualPath, y.VirtualPath, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(VirtualFile obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }
            return obj.VirtualPath.GetHashCode();
        }

        public int Compare(VirtualFile x, VirtualFile y)
        {
            if (x == null)
            {
                throw new ArgumentNullException(nameof(x));
            }
            if (y == null)
            {
                throw new ArgumentNullException(nameof(y));
            }

            return string.Compare(x.VirtualPath, y.VirtualPath, StringComparison.OrdinalIgnoreCase);
        }
    }
}
