// MIT License.

using System.Text;

namespace Microsoft.AspNetCore.SystemWebAdapters.UI.RuntimeCompilation;

public readonly struct PagePath
{
    public PagePath(string path)
        : this(Path.GetDirectoryName(path), Path.GetFileName(path))
    {
    }

    public PagePath(string? directory, string path)
    {
        FilePath = path;

        var trimmed = directory is null ? path : Combine(directory, path);
        UrlPath = trimmed;
        ClassName = ConvertPathToClassName(trimmed);
    }

    private static string Combine(string directory, string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return string.Empty;
        }

        var sb = new StringBuilder(path);

        if (sb[0] == '~')
        {
            sb.Remove(0, 1);
        }
        else
        {
            if (sb[0] != '/')
            {
                sb.Insert(0, '/');
            }

            sb.Insert(0, directory);
        }

        if (sb[0] != '/')
        {
            sb.Insert(0, '/');
        }

        sb.Replace("\\", "/");
        sb.Replace("//", "/");
        sb.Replace("//", "/");

        return sb.ToString();
    }

    public string UrlPath { get; }

    public string ClassName { get; }

    public string FilePath { get; }

    private static string ConvertPathToClassName(string input)
    {
        var sb = new StringBuilder(input);

        sb.Replace("~", string.Empty);
        sb.Replace(".", "_");
        sb.Replace("/", "_");
        sb.Replace("\\", "_");

        return sb.ToString();
    }

}
