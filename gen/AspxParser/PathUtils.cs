// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.IO;
using System.Runtime.InteropServices;

namespace Microsoft.AspNetCore.SystemWebAdapters.UI.AspxParser;

public static class PathUtils
{
    private static bool? isRunningOnLinux;
    private static bool? isCoreApp;

    public const int MaxDirLength = 248 - 1;
    public const int MaxPathLength = 260 - 1;

    public static bool IsWindows
    {
        get
        {
            if (!isRunningOnLinux.HasValue)
            {
                isRunningOnLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            }

            return isRunningOnLinux.Value;
        }
    }

    public static bool IsCoreApp
    {
        get
        {
            if (!isCoreApp.HasValue)
            {
                isCoreApp = RuntimeInformation.FrameworkDescription.Contains(".NET Core");
            }

            return isCoreApp.Value;
        }
    }

    public static string NormalizeFilePath(this string path) => path.NormalizePath(false);

    public static string NormalizeDirPath(this string path, bool force = false) => path.NormalizePath(true, force);

    private static string NormalizePath(this string path, bool isDirectory = true, bool force = false)
    {
        if (IsWindows && !IsCoreApp && !path.StartsWith(@"\\?\") &&
            (path.Length > (isDirectory ? MaxDirLength : MaxPathLength) || force))
        {
            if (path.StartsWith(@"\\"))
            {
                return $@"\\?\UNC\{path.Remove(2)}";
            }

            path = path.NormalizeDirSeparator();

            return $@"\\?\{path}";
        }

        return path.NormalizeDirSeparator();
    }

    public static string NormalizeDirSeparator(this string path)
    {
        var notPlatformSeparator = IsWindows ? "/" : "\\";

        if (path.Contains(notPlatformSeparator))
        {
            return path.Replace(notPlatformSeparator, Path.DirectorySeparatorChar.ToString());
        }

        return path;
    }
}
