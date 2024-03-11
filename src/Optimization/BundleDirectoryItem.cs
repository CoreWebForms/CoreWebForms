// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using System.Text.RegularExpressions;

namespace System.Web.Optimization
{
    internal sealed class BundleDirectoryItem : BundleItem
    {
        // Assumed to always exist (do validation before constructor)
        public BundleDirectoryItem(string path, string searchPattern, PatternType patternType, bool searchSubdirectories, IList<IItemTransform> transforms)
            : base(path, transforms)
        {
            SearchPattern = searchPattern;
            PatternType = patternType;
            SearchSubdirectories = searchSubdirectories;
        }

        public string SearchPattern { get; set; }
        public PatternType PatternType { get; set; }
        public bool SearchSubdirectories { get; set; }

        /// <summary>
        /// Recursively add all subdirectories to the cache dependencies
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="context"></param>
        private static void AddAllSubdirectories(VirtualDirectory dir, BundleContext context)
        {
            context.CacheDependencyDirectories.Add(dir.VirtualPath);
            foreach (VirtualDirectory subDir in dir.Directories)
            {
                AddAllSubdirectories(subDir, context);
            }
        }

        public void ProcessDirectory(BundleContext context, string directoryVirtualPath, VirtualDirectory dirInfo, List<BundleFile> files)
        {
            IEnumerable<VirtualFile> directoryFiles;
            Regex regEx;
            switch (PatternType)
            {
                // We used to be able to just call DirectoryInfo.GetFiles,
                // now we have to add support for * and {version} syntax on top of VPP
                case PatternType.Version:
                    regEx = PatternHelper.BuildRegex(SearchPattern);
                    directoryFiles = dirInfo.Files.Cast<VirtualFile>().Where(file => regEx.IsMatch(file.Name));
                    break;
                case PatternType.All:
                    directoryFiles = dirInfo.Files.Cast<VirtualFile>();
                    break;
                case PatternType.Exact:
                    directoryFiles = dirInfo.Files.Cast<VirtualFile>().Where(file => string.Equals(file.Name, SearchPattern, StringComparison.OrdinalIgnoreCase));
                    break;
                case PatternType.Suffix:
                case PatternType.Prefix:
                default:
                    regEx = PatternHelper.BuildWildcardRegex(SearchPattern);
                    directoryFiles = dirInfo.Files.Cast<VirtualFile>().Where(file => regEx.IsMatch(file.Name));
                    break;
            }

            // Sort the directory files so we get deterministic order
            directoryFiles = directoryFiles.OrderBy(file => file, VirtualFileComparer.Instance);

            var filterList = new List<BundleFile>();
            foreach (var file in directoryFiles)
            {
                filterList.Add(new BundleFile(System.IO.Path.Combine(directoryVirtualPath, file.Name), file, Transforms));
            }
            files.AddRange(context.BundleCollection.DirectoryFilter.FilterIgnoredFiles(context, filterList));

            // Need to recurse on subdirectories if requested
            if (SearchSubdirectories)
            {
                foreach (VirtualDirectory subDir in dirInfo.Directories)
                {
                    ProcessDirectory(context, System.IO.Path.Combine(directoryVirtualPath, subDir.Name), subDir, files);
                }
            }
        }

        /// <summary>
        /// Assumption is VirtualPathProvider.DirectoryExists is true for Path
        /// </summary>
        /// <param name="files"></param>
        /// <param name="context"></param>
        public override void AddFiles(List<BundleFile> files, BundleContext context)
        {
            Debug.Assert(context.VirtualPathProvider.DirectoryExists(VirtualPath));
            var dirInfo = context.VirtualPathProvider.GetDirectory(VirtualPath);
            ProcessDirectory(context, VirtualPath, dirInfo, files);

            if (context != null)
            {
                // Always add directories to the cache dependency since new files could show up that match a pattern
                if (SearchSubdirectories)
                {
                    AddAllSubdirectories(dirInfo, context);
                }
                else
                {
                    context.CacheDependencyDirectories.Add(VirtualPath);
                }
            }
        }
    }
}
