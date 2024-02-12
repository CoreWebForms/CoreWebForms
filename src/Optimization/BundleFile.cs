// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Web.Hosting;

namespace System.Web.Optimization
{
    /// <summary>
    /// Represents a single file within a bundle.
    /// </summary>
    public class BundleFile
    {
        private List<IItemTransform> _transforms = new List<IItemTransform>();

        /// <summary>
        /// Constructor taking a list of transforms for the file
        /// </summary>
        /// <param name="includedVirtualPath"></param>
        /// <param name="file"></param>
        /// <param name="transforms"></param>
        public BundleFile(string includedVirtualPath, VirtualFile file, IList<IItemTransform> transforms)
        {
            VirtualFile = file;
            if (transforms != null)
            {
                _transforms.AddRange(transforms);
            }
            IncludedVirtualPath = includedVirtualPath;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="includedVirtualPath"></param>
        /// <param name="file"></param>
        public BundleFile(string includedVirtualPath, VirtualFile file) : this(includedVirtualPath, file, null) { }

        private VirtualFile _virtualFile;
        /// <summary>
        /// VirtualFile
        /// </summary>
        public VirtualFile VirtualFile
        {
            get
            {
                return _virtualFile;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }
                _virtualFile = value;
            }
        }

        /// <summary>
        /// The Included virtualPath that caused this file to be included in the bundle
        /// i.e. bundle.Include("~/*.js") or bundle.Include("~/foo.js")
        /// </summary>
        public string IncludedVirtualPath
        {
            get;
            set;
        }

        /// <summary>
        /// Transforms that apply to this specific file
        /// </summary>
        public IList<IItemTransform> Transforms
        {
            get
            {
                return _transforms;
            }
        }

        /// <summary>
        /// Applies the transforms to the file and returns the transformed stream
        /// </summary>
        /// <returns></returns>
        public string ApplyTransforms()
        {
            string item;
            using (var reader = new StreamReader(VirtualFile.Open()))
            {
                item = reader.ReadToEnd();
            }
            if (Transforms != null && Transforms.Count > 0)
            {
                foreach (var transform in Transforms)
                {
                    item = transform.Process(IncludedVirtualPath, item);
                }
            }
            return item;
        }
    }

}
