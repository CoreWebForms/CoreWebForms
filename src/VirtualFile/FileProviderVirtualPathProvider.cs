// MIT License.

using System.Collections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;

namespace System.Web;

internal class FileProviderVirtualPathProvider(IFileProvider files) : VirtualPathProvider
{
    private static string NormalizePath(string input) => VirtualPath.Create(input).Path;

    public override VirtualFile GetFile(string virtualPath) => new File(files, NormalizePath(virtualPath));

    public override VirtualDirectory GetDirectory(string virtualDir) => new Dir(files, NormalizePath(virtualDir));

    public override bool DirectoryExists(string virtualDir) => files.GetFileInfo(NormalizePath(virtualDir)) is { IsDirectory: true, Exists: true };

    public override bool FileExists(string virtualPath) => files.GetFileInfo(NormalizePath(virtualPath)) is { IsDirectory: false, Exists: true };

    private sealed class File(IFileProvider files, string path) : VirtualFile(path)
    {
        public override Stream Open() => files.GetFileInfo(VirtualPath).CreateReadStream();
    }

    private sealed class Dir(IFileProvider files, string path) : VirtualDirectory(path)
    {
        public override IEnumerable Directories => GetAll(false, true);

        public override IEnumerable Files => GetAll(true, false);

        public override IEnumerable Children => GetAll(true, true);

        private IEnumerable GetAll(bool returnFiles, bool returnDirectories)
        {
            foreach (var item in files.GetDirectoryContents(VirtualPath))
            {
                if (returnFiles && !item.IsDirectory)
                {
                    yield return new File(files, Path.Combine(VirtualPath, item.Name));
                }

                else if (returnDirectories && item.IsDirectory)
                {
                    yield return new Dir(files, Path.Combine(VirtualPath, item.Name));
                }
            }
        }
    }
}
