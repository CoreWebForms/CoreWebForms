// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Hosting;

namespace System.Web.Optimization
{
    /// <summary>
    /// Default <see cref="IBundleOrderer"/> which orders files in a bundled using <see cref="BundleCollection.FileSetOrderList"/>.
    /// </summary>
    public class DefaultBundleOrderer : IBundleOrderer
    {
        // We only need one of these since everything it does is currently static
        internal static DefaultBundleOrderer Instance = new DefaultBundleOrderer();

        // build a hash of files by name (Note, we group files that share the same name together)
        private static Dictionary<string, HashSet<BundleFile>> BuildFileMap(IEnumerable<BundleFile> files)
        {
            var fileMap = new Dictionary<string, HashSet<BundleFile>>(StringComparer.OrdinalIgnoreCase);
            foreach (var f in files)
            {
                var key = f.VirtualFile.Name;
                if (fileMap.ContainsKey(key))
                {
                    fileMap[key].Add(f);
                }
                else
                {
                    var l = new HashSet<BundleFile>(BundleFileComparer.Instance);
                    l.Add(f);
                    fileMap[key] = l;
                }
            }
            return fileMap;
        }

        private static void AddOrderingFiles(BundleFileSetOrdering ordering, IEnumerable<BundleFile> files, Dictionary<string, HashSet<BundleFile>> fileMap, HashSet<VirtualFile> foundFiles, List<BundleFile> result)
        {
            foreach (var fileName in ordering.Files)
            {
                // If the file ends in a wildcard we need to do special logic
                if (fileName.EndsWith("*", StringComparison.OrdinalIgnoreCase))
                {
                    // Adds all files that match the prefix of the filename (i.e. jquery-*) 
                    var prefix = fileName.Substring(0, fileName.Length - 1);

                    // iterate thru all the files and add matches
                    var matchedFiles = files.Where(f => !foundFiles.Contains(f.VirtualFile) && f.VirtualFile.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
                    foreach (var f in matchedFiles)
                    {
                        result.Add(f);
                        foundFiles.Add(f.VirtualFile);
                    }
                }
                else
                {
                    if (fileMap.ContainsKey(fileName))
                    {
                        // Sort the hashed files to guarantee an ordering
                        var sortedFiles = new List<BundleFile>(fileMap[fileName]);
                        sortedFiles.Sort(BundleFileComparer.Instance);
                        foreach (var fi in sortedFiles)
                        {
                            if (!foundFiles.Contains(fi.VirtualFile))
                            { // Only add the file to the bundle once
                                result.Add(fi);
                                foundFiles.Add(fi.VirtualFile);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Reorder files that match patterns specified in <see cref="BundleCollection.FileSetOrderList"/> so that they are bundled before any 
        /// files that do not match.
        /// </summary>
        public virtual IEnumerable<BundleFile> OrderFiles(BundleContext context, IEnumerable<BundleFile> files)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            if (files == null)
            {
                throw new ArgumentNullException(nameof(files));
            }

            // No need to do anything if no filesets in order list
            if (context.BundleCollection.FileSetOrderList.Count == 0)
            {
                return files;
            }

            // Goal is to return a list of files in FileSetOrderList where each registered file set is ordered if it exists
            var result = new List<BundleFile>();
            var fileList = new List<BundleFile>(files);

            var fileMap = BuildFileMap(fileList);
            if (fileMap.Count == 0)
            {
                return result;
            }

            var foundFiles = new HashSet<VirtualFile>(VirtualFileComparer.Instance);

            // For each ordering if we find a file, output all files that match that name
            foreach (var ordering in context.BundleCollection.FileSetOrderList)
            {
                AddOrderingFiles(ordering, fileList, fileMap, foundFiles, result);
            }

            // Last step, add all unused files to the final list
            foreach (var f in fileList)
            {
                if (!foundFiles.Contains(f.VirtualFile))
                {
                    result.Add(f);
                    foundFiles.Add(f.VirtualFile);
                }
            }

            return result;
        }
    }
}
