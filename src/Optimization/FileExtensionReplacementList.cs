// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Web.Hosting;

namespace System.Web.Optimization
{
    /// <summary>
    /// A set of file extensions that will be used to select different files based on the <see cref="OptimizationMode"/>.
    /// </summary>
    /// <remarks>
    /// As an example, a file extension replacement entry for "min" means foo.min.js will be used instead of foo.js
    /// </remarks>
    public class FileExtensionReplacementList
    {
        private List<Entry> _entries = new List<Entry>();

        // Just for unit tests to verify defaults
        internal int Count
        {
            get
            {
                return _entries.Count;
            }
        }

        internal Entry this[int index]
        {
            get
            {
                return _entries[index];
            }
        }

        /// <summary>
        /// Adds a file extension which will be applied regardless of <see cref="OptimizationMode"/>
        /// </summary>
        /// <param name="extension">File extension string.</param>
        public void Add(string extension)
        {
            Add(extension, OptimizationMode.Always);
        }

        /// <summary>
        /// Add a file extension for a specified <see cref="OptimizationMode"/>.
        /// </summary>
        /// <param name="extension">File extension string.</param>
        /// <param name="mode"><see cref="OptimizationMode"/> in which to apply the file extension replacement.</param>
        public void Add(string extension, OptimizationMode mode)
        {
            _entries.Add(new Entry(extension, mode));
        }

        /// <summary>
        /// Clears file extension replacements.
        /// </summary>
        public void Clear()
        {
            _entries.Clear();
        }

        // Look for <fileName>.<replacementExtension>(.<ext>) in the diretory path
        private static BundleFile FindReplacementFile(BundleContext context, BundleFile file, string replacementExtension)
        {
            var directoryPath = Path.GetDirectoryName(file.VirtualFile.VirtualPath);
            var extension = Path.GetExtension(file.VirtualFile.Name);
            var fileName = Path.GetFileNameWithoutExtension(file.VirtualFile.Name);
            var replacementFileName = fileName + "." + replacementExtension;
            if (extension.Length > 0)
            {
                replacementFileName += extension;
            }

            // NOTE: VirtualPathUtility.Combine does not work as it uses '/' while
            // the default VPP requires '\' to find files, this might have result
            // in issues with Custom VPP implementations and this feature
            var replacementFilePath = Path.Combine(directoryPath, replacementFileName);
            var replacementIncludePath = Path.Combine(Path.GetDirectoryName(file.IncludedVirtualPath), replacementFileName);
            // Need to fixup the slashes
            replacementIncludePath = replacementIncludePath.Replace('\\', '/');

            // NOTE: We cache every bundle response, so we shouldn't be hitting FileExists too often
            if (context.VirtualPathProvider.FileExists(replacementFilePath))
            {
                return new BundleFile(replacementIncludePath,
                    context.VirtualPathProvider.GetFile(replacementFilePath));
            }
            return null;
        }

        /// <summary>
        /// Uses the file extension replacements to select the correct version from a set of files.
        /// </summary>
        /// <param name="context">The <see cref="BundleContext"/> object that contains state for both the framework configuration and the HTTP request.</param>
        /// <param name="files">The files contained in the bundle.</param>
        /// <returns></returns>
        public virtual IEnumerable<BundleFile> ReplaceFileExtensions(BundleContext context, IEnumerable<BundleFile> files)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (files == null || _entries.Count == 0)
            {
                return files;
            }

            var replacedFiles = new List<BundleFile>();
            var foundFiles = new HashSet<VirtualFile>(VirtualFileComparer.Instance);

            // Need to scan for any files(<name>.<ext> that match <name>.<replacementList>.<ext> and use that instead
            BundleFile replacedFile;
            foreach (var file in files)
            {
                replacedFile = null;

                // Make sure to skip the actual replacement files if they've already been added
                if (foundFiles.Contains(file.VirtualFile))
                {
                    continue;
                }

                // Look for each extension replacement in the same directory as the requested file
                var useReplacement = false;
                foreach (var entry in _entries)
                {
                    // Skip entries that aren't applicable to the current optimization mode
                    if (!entry.UseEntry(context.EnableOptimizations))
                    {
                        continue;
                    }
                    var replacementExtension = entry.Extension;
                    replacedFile = FindReplacementFile(context, file, replacementExtension);
                    if (replacedFile != null)
                    {
                        if (!foundFiles.Contains(replacedFile.VirtualFile))
                        {
                            replacedFiles.Add(replacedFile);
                            foundFiles.Add(replacedFile.VirtualFile);
                        }
                        useReplacement = true;
                        break;
                    }
                }

                if (!useReplacement)
                {
                    replacedFiles.Add(file);
                    foundFiles.Add(file.VirtualFile);
                }
            }

            return replacedFiles;
        }

        internal sealed class Entry
        {
            public Entry(string extension, OptimizationMode mode)
            {
                Extension = extension;
                Mode = mode;
            }

            public string Extension { get; set; }
            public OptimizationMode Mode { get; set; }

            /// <summary>
            /// Returns true if the entry should be used in this optimization mode
            /// </summary>
            /// <param name="optimizationMode"></param>
            /// <returns></returns>
            public bool UseEntry(bool optimizationMode)
            {
                switch (Mode)
                {
                    case OptimizationMode.Always:
                        return true;
                    case OptimizationMode.WhenEnabled:
                        return optimizationMode;
                    case OptimizationMode.WhenDisabled:
                        return !optimizationMode;
                }
                return false;
            }

        }
    }
}
