// MIT License.

using System.Collections;
using System.Text;

namespace System.Web;

internal sealed class VirtualPath
{
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

    public static string Resolve(string url)
    {
        if (!url.StartsWith('~'))
        {
            return url;
        }

        var vdir = HttpRuntime.AppDomainAppVirtualPath;

        var sb = new StringBuilder(url, 1, url.Length - 1, url.Length + vdir.Length);

        if (sb.Length == 0 || sb[0] != '/')
        {
            sb.Insert(0, '/');
        }

        sb.Insert(0, vdir);
        sb.Replace("//", "/");

        return sb.ToString();
    }

    public string Path { get; }

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

    public string VirtualPathString => Path;

    public string Extension => IO.Path.GetExtension(Path);

    public string FileName => IO.Path.GetFileName(Path);

    public object AppRelativeVirtualPathString => Path;

    public static implicit operator VirtualPath(string path) => new(path);
    public static implicit operator string(VirtualPath vpath) => vpath.Path;

    public static VirtualPath CreateAllowNull(string path) => new(path);

    internal static VirtualPath CreateNonRelativeAllowNull(string v) => v;

    internal static string GetAppRelativeVirtualPathStringOrEmpty(VirtualPath vpath)
        => vpath.Path;

    internal static string GetVirtualPathString(VirtualPath vpath)
        => vpath?.Path;

    internal string GetAppRelativeVirtualPathString(VirtualPath templateControlVirtualPath)
    {
        throw new NotImplementedException();
    }

    internal static VirtualPath CreateNonRelative(string value)
    {
        return value;
    }

    internal Stream OpenFile() => File.OpenRead(Path);

    internal static VirtualPath Create(string filename) => filename;

    internal bool FileExists() => File.Exists(Path);

    internal string MapPath()
    {
        throw new NotImplementedException();
    }

    internal static VirtualPath CreateTrailingSlash(string virtualPath)
    {
        throw new NotImplementedException();
    }
}

public class VirtualPathProvider
{
    VirtualPath _virtualPath;
    public VirtualPathProvider(string path)
    {
        _virtualPath = new VirtualPath(path);
    }

    internal static VirtualPath CombineVirtualPathsInternal(VirtualPath templateControlVirtualPath, VirtualPath masterPageFile)
    {
        string result;

        if (masterPageFile.Path.StartsWith('~'))
        {
            result = masterPageFile.Path.TrimStart('~');
        }
        else
        {
            result = Path.Combine(Path.GetDirectoryName(templateControlVirtualPath.Path), masterPageFile.Path);
        }

        return result.TrimStart('/');
    }

    public virtual bool FileExists(string virtualPath)
    {
        return _virtualPath.FileExists();
    }

    public virtual Stream Open()
    {
        return _virtualPath.OpenFile();
    }



}

