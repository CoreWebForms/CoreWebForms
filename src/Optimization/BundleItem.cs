// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Web.Hosting;

namespace System.Web.Optimization
{
    internal class BundleItem
    {
        private List<IItemTransform> _transforms = new List<IItemTransform>();

        public BundleItem(string virtualPath) : this(virtualPath, null)
        {
        }

        public BundleItem(string virtualPath, IEnumerable<IItemTransform> transforms)
        {
            VirtualPath = virtualPath;
            if (transforms != null)
            {
                Transforms.AddRange(transforms);
            }
        }

        /// <summary>
        /// VirtualPath to the item in the bundle
        /// </summary>
        public string VirtualPath { get; set; }

        /// <summary>
        /// Transforms that will be applied only to this specific item
        /// </summary>
        public List<IItemTransform> Transforms
        {
            get
            {
                return _transforms;
            }
        }

        /// <summary>
        /// Resolve the actual BundleFile objects that should be part of the bundle.
        /// Virtual so directory items can do the correct thing
        /// </summary>
        /// <param name="files"></param>
        /// <param name="context"></param>
        public virtual void AddFiles(List<BundleFile> files, BundleContext context)
        {
            files.Add(new BundleFile(VirtualPath, context.VirtualPathProvider.GetFile(VirtualPath), Transforms));
        }
    }
}
