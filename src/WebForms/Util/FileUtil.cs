//------------------------------------------------------------------------------
// <copyright file="UrlPath.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * UrlPath class
 *
 * Copyright (c) 1999 Microsoft Corporation
 */

namespace System.Web.Util {
using System.Security.Permissions;
using System.Text;
using System.Runtime.Serialization.Formatters;
using System.Runtime.InteropServices;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Web.Hosting;


internal struct FileTimeInfo {
    internal long LastWriteTime;
    internal long Size;

    internal static readonly FileTimeInfo MinValue = new FileTimeInfo(0, 0);

    internal FileTimeInfo(long lastWriteTime, long size) {
        LastWriteTime = lastWriteTime;
        Size = size;
    }

    public override bool Equals(object obj) {
        FileTimeInfo fti;

        if (obj is FileTimeInfo) {
            fti = (FileTimeInfo) obj;
            return (LastWriteTime == fti.LastWriteTime) && (Size == fti.Size);
        }
        else {
            return false;
        }
    }

    public static bool operator == (FileTimeInfo value1, FileTimeInfo value2)
    {
        return (value1.LastWriteTime == value2.LastWriteTime) &&
               (value1.Size == value2.Size);
    }

    public static bool operator != (FileTimeInfo value1, FileTimeInfo value2)
    {
        return !(value1 == value2);
    }

    public override int GetHashCode(){
        return HashCodeCombiner.CombineHashCodes(LastWriteTime.GetHashCode(), Size.GetHashCode());
    }


}

/*
 * Helper methods relating to file operations
 */
internal class FileUtil {

    private FileUtil() {
    }

    internal static bool FileExists(String filename) {
        bool exists = false;

        try {
            exists = File.Exists(filename);
        }
        catch {
        }

        return exists;
    }

    // For a given path, if its beneath the app root, return the first existing directory
    internal static string GetFirstExistingDirectory(string appRoot, string fileName) {
        if (IsBeneathAppRoot(appRoot, fileName)) {
            string existingDir = appRoot;
            do {
                int nextSeparator = fileName.IndexOf(Path.DirectorySeparatorChar, existingDir.Length + 1);
                if (nextSeparator > -1) {
                    string nextDir = fileName.Substring(0, nextSeparator);
                    if (DirectoryExists(nextDir)) {
                        existingDir = nextDir;
                        continue;
                    }
                }
                break;
            } while (true);

            return existingDir;
        }
        return null;
    }

    internal static bool IsBeneathAppRoot(string appRoot, string filePath) {
        if (filePath.Length > appRoot.Length + 1
            && filePath.IndexOf(appRoot, StringComparison.OrdinalIgnoreCase) > -1
            && filePath[appRoot.Length] == Path.DirectorySeparatorChar) {
            return true;
        }
        return false;
    }

    // Remove the final backslash from a directory path, unless it's something like c:\
    internal static String RemoveTrailingDirectoryBackSlash(String path) {

        if (path == null)
            return null;

        int length = path.Length;
        if (length > 3 && path[length - 1] == '\\')
            path = path.Substring(0, length - 1);

        return path;
    }

    private static int _maxPathLength = 259;
    // If the path is longer than the maximum length
    // Trim the end and append the hashcode to it.
    internal static String TruncatePathIfNeeded(string path, int reservedLength) {
        int maxPathLength = _maxPathLength - reservedLength;
        if (path.Length > maxPathLength) {
            //

            path = path.Substring(0, maxPathLength - 13) +
                path.GetHashCode().ToString(CultureInfo.InvariantCulture);
        }

        return path;
    }

    /*
     * Canonicalize the directory, and makes sure it ends with a '\'
     */
    internal static string FixUpPhysicalDirectory(string dir) {
        if (dir == null)
            return null;

        dir = Path.GetFullPath(dir);

        // Append '\' to the directory if necessary.
        if (!StringUtil.StringEndsWith(dir, '\\'))
            dir = dir + @"\";

        return dir;
    }

    // Fail if the physical path is not canonical
    static internal void CheckSuspiciousPhysicalPath(string physicalPath) {
        if (IsSuspiciousPhysicalPath(physicalPath)) {
            throw new HttpException(404, String.Empty);
        }
    }

    // Check whether the physical path is not canonical
    // NOTE: this API throws if we don't have permission to the file.
    // NOTE: The compare needs to be case insensitive (VSWhidbey 444513)
    static internal bool IsSuspiciousPhysicalPath(string physicalPath) {
        bool pathTooLong;

        if (!IsSuspiciousPhysicalPath(physicalPath, out pathTooLong)) {
            return false;
        }

        if (!pathTooLong) {
            return true;
        }

        // physical path too long -> not good because we still need to make
        // it work for virtual path provider scenarios

        // first a few simple checks:
        if (physicalPath.IndexOf('/') >= 0) {
            return true;
        }

        string slashDots = "\\..";
        int idxSlashDots = physicalPath.IndexOf(slashDots, StringComparison.Ordinal);
        if (idxSlashDots >= 0
            && (physicalPath.Length == idxSlashDots + slashDots.Length
                || physicalPath[idxSlashDots + slashDots.Length] == '\\')) {
            return true;
        }

        // the real check is to go right to left until there is no longer path-too-long
        // and see if the canonicalization check fails then

        int pos = physicalPath.LastIndexOf('\\');

        while (pos >= 0) {
            string path = physicalPath.Substring(0, pos);

            if (!IsSuspiciousPhysicalPath(path, out pathTooLong)) {
                // reached a non-suspicious path that is not too long
                return false;
            }

            if (!pathTooLong) {
                // reached a suspicious path that is not too long
                return true;
            }

            // trim the path some more
            pos = physicalPath.LastIndexOf('\\', pos-1);
        }

        // backtracted to the end without reaching a non-suspicious path
        // this is suspicious (should happen because app root at least should be ok)
        return true;
    }

    private static readonly char[] s_invalidPathChars = Path.GetInvalidPathChars();

    // VSWhidbey 609102 - Medium trust apps may hit this method, and if the physical path exists,
    // Path.GetFullPath will seek PathDiscovery permissions and throw an exception.
    static internal bool IsSuspiciousPhysicalPath(string physicalPath, out bool pathTooLong) {
        bool isSuspicious;

        // DevDiv 340712: GetConfigPathData generates n^2 exceptions where n is number of incorrectly placed '/'
        // Explicitly prevent frequent exception cases since this method is called a few times per url segment
        if ((physicalPath != null) &&
             (physicalPath.Length > _maxPathLength ||
             physicalPath.IndexOfAny(s_invalidPathChars) != -1 ||
             // Contains ':' at any position other than 2nd char
             (physicalPath.Length > 0 && physicalPath[0] == ':') ||
             (physicalPath.Length > 2 && physicalPath.IndexOf(':', 2) > 0))) {

            // see comment below
            pathTooLong = true;
            return true;
        }

        try {
            isSuspicious = !String.IsNullOrEmpty(physicalPath) &&
                String.Compare(physicalPath, Path.GetFullPath(physicalPath),
                    StringComparison.OrdinalIgnoreCase) != 0;
            pathTooLong = false;
        }
        catch (PathTooLongException) {
            isSuspicious = true;
            pathTooLong = true;
        }
        catch (NotSupportedException) {
            // see comment below -- we do the same for ':'
            isSuspicious = true;
            pathTooLong = true;
        }
        catch (ArgumentException) {
            // DevDiv Bugs 152256:  Illegal characters {",|} in path prevent configuration system from working.
            // We need to catch this exception and conservatively assume that the path is suspicious in
            // such a case.
            // We also set pathTooLong to true because at this point we do not know if the path is too long
            // or not. If we assume that pathTooLong is false, it means that our path length enforcement
            // is bypassed by using URLs with illegal characters. We do not want that. Moreover, returning
            // pathTooLong = true causes the current logic to peel of URL fragments, which can also find a
            // path without illegal characters to retrieve the config.
            isSuspicious = true;
            pathTooLong = true;
        }

        return isSuspicious;
    }

    static bool HasInvalidLastChar(string physicalPath) {
        // see VSWhidbey #108945
        // We need to filter out directory names which end
        // in " " or ".".  We want to treat path names that
        // end in these characters as files - however, Windows
        // will strip these characters off the end of the name,
        // which may result in the name being treated as a
        // directory instead.

        if (String.IsNullOrEmpty(physicalPath)) {
            return false;
        }

        char lastChar = physicalPath[physicalPath.Length - 1];
        return lastChar == ' ' || lastChar == '.';
    }

    internal static bool DirectoryExists(String dirname) {
        bool exists = false;
        dirname = RemoveTrailingDirectoryBackSlash(dirname);
        if (HasInvalidLastChar(dirname))
            return false;

        try {
            exists = Directory.Exists(dirname);
        }
        catch {
        }

        return exists;
    }

    internal static bool DirectoryAccessible(String dirname) {
        bool accessible = false;
        dirname = RemoveTrailingDirectoryBackSlash(dirname);
        if (HasInvalidLastChar(dirname))
            return false;

        try {
            accessible = (new DirectoryInfo(dirname)).Exists;
        }
        catch {
        }

        return accessible;
    }

    private static Char[] _invalidFileNameChars = Path.GetInvalidFileNameChars();
    internal static bool IsValidDirectoryName(String name) {
        if (String.IsNullOrEmpty(name)) {
            return false;
        }

        if (name.IndexOfAny(_invalidFileNameChars, 0) != -1) {
            return false;
        }

        if (name.Equals(".") || name.Equals("..")) {
            return false;
        }

        return true;
    }
}

}
