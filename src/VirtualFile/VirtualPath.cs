// MIT License.

using System.Text;
using System.Web.Hosting;
using System.Web.Util;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace System.Web;

public sealed class VirtualPath
{
    public IFileProvider FileProvider => HttpRuntime.WebObjectActivator.GetRequiredService<IWebHostEnvironment>().ContentRootFileProvider;
    public VirtualPath Parent
    {
        get
        {
            var idx = Path.LastIndexOfAny(new[] { '/', '\\' });

            if (idx == -1)
            {
                return "/";
            }

            var result = Path.Substring(0, idx);

            return result + "/";
        }
    }

    public VirtualPath(string path)
    {
        Path = Resolve(path);
    }

    public string GetCacheKey() => Path;

    public bool HasTrailingSlash => Path.EndsWith("/");

    public bool DirectoryExists() {
        // TODO: Check
        //return HostingEnvironment.VirtualPathProvider.DirectoryExists(this);
        return FileProvider.GetDirectoryContents(Path).Exists;
    }

    public static string Resolve(string url)
    {
        if (!url.StartsWith('~'))
        {
            return Normalize(url);
        }

        var vdir = HttpRuntime.AppDomainAppVirtualPath;

        var sb = new StringBuilder(url, 1, url.Length - 1, url.Length + vdir.Length);

        if (sb.Length == 0 || sb[0] != '/')
        {
            sb.Insert(0, '/');
        }

        sb.Insert(0, vdir);

        Normalize(sb);

        return sb.ToString();
    }

    private static string Normalize(string str)
    {
        var sb = new StringBuilder(str);
        Normalize(sb);
        return sb.ToString();
    }

    private static void Normalize(StringBuilder sb)
    {
        sb.Replace("//", "/");
        sb.Replace("\\", "/");
    }

    public string Path { get; }

    public override string ToString() => Path;

    public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Path);

    public override bool Equals(object obj) => obj is VirtualPath other && Equals(other);

    public bool Equals(VirtualPath other) => string.Equals(Path, other?.Path, StringComparison.OrdinalIgnoreCase);

    public Stream OpenFile(VirtualPathProvider provider)
    {
        return provider.GetFile(FileName)?.Open();
    }

    public string VirtualPathStringNoTrailingSlash
    {
        get
        {
            if (Path.EndsWith("/"))
            {
                return Path;
            }

            return Path + "/";
        }
    }
    public bool IsRelative
    {
        get
        {
            // Note that we don't need to check for "~/", since _virtualPath never contains
            // app relative paths (_appRelativeVirtualPath does)
            return Path != null && Path[0] != '/';
        }
    }

    public bool IsWithinAppRoot
    {
        get
        {
            return IsRelative;
        }
    }

    public VirtualPath CombineWithAppRoot()
    {
        return new VirtualPath(HttpRuntime.AppDomainAppPath).Combine(this);
    }

    public VirtualPath SimpleCombineWithDir(VirtualPath relativePath) => Combine(relativePath);
    public VirtualPath Combine(VirtualPath relativePath)
    {
        if (relativePath == null)
        {
            throw new ArgumentNullException(nameof(relativePath));
        }

        // If it's not relative, return it unchanged
        if (!relativePath.IsRelative)
        {
            return relativePath;
        }

        // The base of the combine should never be relative
        FailIfRelativePath();

        // Combine it with the relative
        var virtualPath = UrlPath2.Combine(Path, relativePath.VirtualPathString);

        // Set the appropriate virtual path in the new object
        return new VirtualPath(virtualPath);
    }

    internal void FailIfRelativePath()
    {
        if (IsRelative)
        {
            throw new ArgumentException($"Must be non-relative path - {Path}");
        }
    }

    public string VirtualPathString => Path;

    public string Extension => IO.Path.GetExtension(Path);

    public string FileName => IO.Path.GetFileName(Path);

    public string AppRelativeVirtualPathString => Path;

    public static implicit operator VirtualPath(string path) => new(path);
    public static implicit operator string(VirtualPath vpath) => vpath?.Path;

    public static VirtualPath CreateAllowNull(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        return new(path);
    }

    internal static VirtualPath CreateNonRelativeAllowNull(string v) => v;

    internal static string GetAppRelativeVirtualPathStringOrEmpty(VirtualPath vpath)
        => vpath.Path;

    internal static string GetVirtualPathString(VirtualPath vpath)
        => vpath?.Path;

    internal string GetAppRelativeVirtualPathString(VirtualPath templateControlVirtualPath)
    {
        // Check if the current path is already application-relative
        if (Path.StartsWith("~") || templateControlVirtualPath.IsRelative)
        {
            return Path; // Already application-relative, return as is.
        }

        if (templateControlVirtualPath != null && !string.IsNullOrEmpty(templateControlVirtualPath.Path))
        {
            // If a template control's virtual path is provided, use it to resolve the current path
            // This assumes that templateControlVirtualPath is an application-relative path
            var basePath = templateControlVirtualPath.Path;
            if (!basePath.EndsWith("/"))
            {
                basePath += "/";
            }

            // Combine the base path with the current path to create a new application-relative path
            var combinedPath = VirtualPathUtility.Combine(basePath, Path);
            return "~/" + combinedPath.TrimStart('/'); // Ensure the result is application-relative
        }

        // Fallback to converting the current path to an application-relative path using the root
        return "~/" + Path.TrimStart('/');
    }

    internal static VirtualPath CreateNonRelative(string value)
    {
        return value;
    }

    internal Stream OpenFile(IFileProvider fileProvider) => fileProvider.GetFileInfo(Path).CreateReadStream();

    internal static VirtualPath Create(string filename) => filename;

    public bool FileExists() => FileProvider.GetFileInfo(Path).Exists;
    internal bool FileExists(IFileProvider fileProvider) => fileProvider.GetFileInfo(Path).Exists;

    public string MapPath() => FileProvider.GetFileInfo(Path).PhysicalPath;
    internal string MapPathInternal() => MapPath();

    public VirtualDirectory GetDirectory() {
        // TODO: Migration
        // Debug.Assert(this.HasTrailingSlash);
        // return HostingEnvironment.VirtualPathProvider.GetDirectory(this);
        return new FileProviderVirtualPathProvider(FileProvider).GetDirectory(this);
    }

    public static bool operator == (VirtualPath v1, VirtualPath v2) {
        return Equals(v1, v2);
    }

    public static bool operator != (VirtualPath v1, VirtualPath v2) {
        return !Equals(v1, v2);
    }

    public void FailIfNotWithinAppRoot()
    {
        if (!IsWithinAppRoot)
        {
            throw new ArgumentException($"{Path} must be within app root");
        }
    }
}
