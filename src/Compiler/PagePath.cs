// MIT License.

using System;
using System.Text;

namespace Microsoft.AspNetCore.SystemWebAdapters.Compiler;

public readonly struct PagePath
{
    public PagePath(string path)
    {
        File = path;

        var trimmed = path.Trim('/', '~', '\\');
        Path = EnsureStartsWithSlash(trimmed);
        ClassName = ConvertPathToClassName(trimmed);
    }

    private static string EnsureStartsWithSlash(string path)
    {
        if (!path.StartsWith("/", StringComparison.OrdinalIgnoreCase))
        {
            path = "/" + path;
        }

        return path;
    }

    public string Path { get; }

    public string ClassName { get; }

    public string File { get; }

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
