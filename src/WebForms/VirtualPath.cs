// MIT License.

namespace System.Web;

internal sealed class VirtualPath
{
    public VirtualPath Parent => Directory.GetParent(Path)!.FullName;

    public VirtualPath(string path)
    {
        Path = path;
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

    public string AppRelativeVirtualPathStringOrNull => IO.Path.GetRelativePath(AppContext.BaseDirectory, Path);

    public object AppRelativeVirtualPathString => Path;

    public static implicit operator VirtualPath(string path) => new(path);
    public static implicit operator string(VirtualPath vpath) => vpath.Path;

    public static VirtualPath CreateAllowNull(string path) => new(path);

    internal static VirtualPath CreateNonRelativeAllowNull(string v) => v;

    internal static string GetAppRelativeVirtualPathStringOrEmpty(VirtualPath vpath)
        => vpath.Path;

    internal static string GetVirtualPathString(VirtualPath vpath)
        => vpath.Path;

    internal string GetAppRelativeVirtualPathString(VirtualPath templateControlVirtualPath)
    {
        throw new NotImplementedException();
    }

    internal VirtualPath CreateNonRelative(string value)
    {
        throw new NotImplementedException();
    }

    internal Stream OpenFile() => File.OpenRead(Path);

    internal static VirtualPath Create(string filename) => filename;

    internal bool FileExists() => File.Exists(Path);

    internal string MapPath()
    {
        throw new NotImplementedException();
    }
}

internal static class VirtualPathProvider
{
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

        return result;
    }
}

