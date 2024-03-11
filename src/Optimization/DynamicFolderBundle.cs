// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Optimization.Resources;

namespace System.Web.Optimization
{
    /// <summary>
    /// Appends virtual urls to directories which bundle files in that directory.
    /// </summary>
    /// <remarks>
    /// As an example, for a subdirectory /Scripts that contains many js files, the declaration DynamicFolderBundle("js", "*.js") would cause 
    /// all JavaScript files in the /Scripts directory to be bundled together and accessed by the virtual path ~/Scripts/js.
    /// </remarks>
    public class DynamicFolderBundle : Bundle
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicFolderBundle"/> class.
        /// </summary>
        /// <param name="pathSuffix">The suffix appended to the directory name in the <see cref="DynamicFolderBundle"/> virtual path.</param>
        /// <param name="searchPattern">The pattern used for including files in the bundle.</param>
        public DynamicFolderBundle(string pathSuffix, string searchPattern)
            : this(pathSuffix, searchPattern, false, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicFolderBundle"/> class.
        /// </summary>
        /// <param name="pathSuffix">The suffix appended to the directory name in the <see cref="DynamicFolderBundle"/> virtual path.</param>
        /// <param name="searchPattern">The pattern used for including files in the bundle.</param>
        /// <param name="transforms">A list of <see cref="IBundleTransform"/> objects which process the contents of the bundle in the order which they are added.</param>
        public DynamicFolderBundle(string pathSuffix, string searchPattern, params IBundleTransform[] transforms)
            : this(pathSuffix, searchPattern, false, transforms)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicFolderBundle"/> class.
        /// </summary>
        /// <param name="pathSuffix">The suffix appended to the directory name in the <see cref="DynamicFolderBundle"/> virtual path.</param>
        /// <param name="searchPattern">The pattern used for including files in the bundle.</param>
        /// <param name="searchSubdirectories">Specifies whether to recursively search subdirectories of referenced virtual directory.</param>
        public DynamicFolderBundle(string pathSuffix, string searchPattern, bool searchSubdirectories)
            : this(pathSuffix, searchPattern, searchSubdirectories, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicFolderBundle"/> class.
        /// </summary>
        /// <param name="pathSuffix">The suffix appended to the directory name in the <see cref="DynamicFolderBundle"/> virtual path.</param>
        /// <param name="searchPattern">The pattern used for including files in the bundle.</param>
        /// <param name="searchSubdirectories">Specifies whether to recursively search subdirectories of referenced virtual directory.</param>
        /// <param name="transforms">A list of <see cref="IBundleTransform"/> objects which process the contents of the bundle in the order which they are added.</param>
        public DynamicFolderBundle(string pathSuffix, string searchPattern, bool searchSubdirectories, params IBundleTransform[] transforms)
        {
            Path = pathSuffix;
            if (IsInvalidRouteUrl(pathSuffix))
            {
                throw new ArgumentException(OptimizationResources.DynamicFolderBundle_InvalidPath, nameof(pathSuffix));
            }
            if (transforms != null)
            {
                foreach (var transform in transforms)
                {
                    Transforms.Add(transform);
                }
            }
            SearchPattern = searchPattern;
            SearchSubdirectories = searchSubdirectories;
        }

        private string _searchPattern;
        /// <summary>
        /// Gets or sets the serach pattern which determines the files included in the bundle.
        /// </summary>
        public string SearchPattern
        {
            get
            {
                return _searchPattern;
            }
            set
            {
                if (ExceptionUtil.IsPureWildcardSearchPattern(value))
                {
                    throw new ArgumentException(OptimizationResources.InvalidWildcardSearchPattern, nameof(value));
                }
                var type = PatternHelper.GetPatternType(value);
                var error = PatternHelper.ValidatePattern(type, value, "value");
                if (error != null)
                {
                    throw error;
                }
                _searchPattern = value;
                PatternType = type;
                InvalidateCacheEntries();
            }
        }

        /// <summary>
        /// Gets or sets an alternate url for the bundle when it is stored in a content delivery network.
        /// </summary>
        /// <remarks>
        /// Because a content delivery network is used with static resources, it does not make sense to ever use CdnPath with dynamic folder bundles.
        /// </remarks>
        public new string CdnPath
        {
            get
            {
                return base.CdnPath;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        internal PatternType PatternType
        {
            get;
            set;
        }

        private bool _searchSubdirectories;
        /// <summary>
        /// Gets or sets whether the search pattern is applied to subdirectories.
        /// </summary>
        public bool SearchSubdirectories
        {
            get
            {
                return _searchSubdirectories;
            }
            set
            {
                _searchSubdirectories = value;
                InvalidateCacheEntries();
            }
        }

        /// <summary>
        /// Generates an enumeration of <see cref="VirtualFile"/> objects that represent the contents of the bundle.
        /// </summary>
        /// <param name="context">The <see cref="BundleContext"/> object that contains state for both the framework configuration and the HTTP request.</param>
        /// <returns>An enumeration of <see cref="VirtualFile"/> objects that represent the contents of the bundle.</returns>
        /// <remarks>
        /// This will return all of the base method's files, and also add any dynamic files found in the requested directory at the end.
        /// </remarks>
        public override IEnumerable<BundleFile> EnumerateFiles(BundleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var files = new List<BundleFile>();
            files.AddRange(base.EnumerateFiles(context));

            var directoryVirtualPath = VirtualPathUtility.GetDirectory(context.BundleVirtualPath);
            if (context.VirtualPathProvider == null || context.VirtualPathProvider.DirectoryExists(directoryVirtualPath))
            {
                new BundleDirectoryItem(directoryVirtualPath, SearchPattern, PatternType, SearchSubdirectories, transforms: null).AddFiles(files, context);
            }
            else
            {
                throw new InvalidOperationException(OptimizationResources.BundleDirectory_does_not_exist);
            }

            return files;
        }

        // Same behavior as SystemWeb's RouteParser
        private static bool IsInvalidRouteUrl(string routeUrl)
        {
            return (routeUrl.StartsWith("~", StringComparison.Ordinal) ||
                routeUrl.StartsWith("/", StringComparison.Ordinal) ||
                routeUrl.Contains('?'));
        }
    }
}
