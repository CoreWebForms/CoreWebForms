// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Web.Hosting;

namespace System.Web.Optimization
{
    // Basic primitive VPP just needed for build time optimization outside of ASP.NET hosting
    // Just calls into System.IO apis
    internal sealed class FileVirtualPathProvider : VirtualPathProvider
    {
        public FileVirtualPathProvider(string applicationPath)
        {
            if (string.IsNullOrEmpty(applicationPath))
            {
                throw ExceptionUtil.ParameterNullOrEmpty("applicationPath");
            }
            ApplicationPath = applicationPath;
        }

        public string ApplicationPath { get; set; }

        // internal for unit tests to turn off existence checks
        private bool _ensureExists = true;
        internal bool EnsureExists
        {
            get
            {
                return _ensureExists;
            }
            set
            {
                _ensureExists = value;
            }
        }

        public string MapPath(string virtualPath)
        {
            // Need to prevent extra slashes
            var prefix = ApplicationPath.EndsWith("/", StringComparison.OrdinalIgnoreCase) ? "~/" : "~";
            return virtualPath.Replace(prefix, ApplicationPath);
        }

        public override bool FileExists(string virtualPath)
        {
            if (EnsureExists)
            {
                return File.Exists(MapPath(virtualPath));
            }
            return true;
        }

        public override bool DirectoryExists(string virtualDir)
        {
            if (EnsureExists)
            {
                return Directory.Exists(MapPath(virtualDir));
            }
            return true;
        }

        public override VirtualFile GetFile(string virtualPath)
        {
            var realPath = MapPath(virtualPath);
            return new FileInfoVirtualFile(realPath, new FileInfo(realPath));
        }

        public override VirtualDirectory GetDirectory(string virtualDir)
        {
            var realPath = MapPath(virtualDir);
            return new DirectoryInfoVirtualDirectory(realPath, new DirectoryInfo(realPath));
        }

        internal sealed class FileInfoVirtualFile : VirtualFile
        {
            public FileInfoVirtualFile(string virtualPath, FileInfo file)
                : base(virtualPath)
            {
                File = file;
            }

            public FileInfo File { get; set; }

            public override Stream Open()
            {
                return File.OpenRead();
            }
        }

        internal sealed class DirectoryInfoVirtualDirectory : VirtualDirectory
        {
            public DirectoryInfoVirtualDirectory(string virtualPath, DirectoryInfo directory)
                : base(virtualPath)
            {
                Directory = directory;
            }

            public DirectoryInfo Directory { get; set; }

            public override Collections.IEnumerable Files
            {
                get
                {
                    var result = new List<VirtualFile>();
                    foreach (var file in Directory.GetFiles())
                    {
                        result.Add(new FileInfoVirtualFile(file.FullName, file));
                    }
                    return result;
                }
            }

            // Not used by Optimization currently
            public override IEnumerable Children
            {
                get { throw new NotImplementedException(); }
            }

            // Not used by Optimization currently
            public override IEnumerable Directories
            {
                get { throw new NotImplementedException(); }
            }
        }
    }
}
