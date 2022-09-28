// MIT License.

namespace System.Web.Util;
internal static class FileUtil
{
    private static readonly Char[] _invalidFileNameChars = Path.GetInvalidFileNameChars();
    internal static bool IsValidDirectoryName(String name)
    {
        if (String.IsNullOrEmpty(name))
        {
            return false;
        }

        if (name.IndexOfAny(_invalidFileNameChars, 0) != -1)
        {
            return false;
        }

        if (name.Equals(".") || name.Equals(".."))
        {
            return false;
        }

        return true;
    }
}
