// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Web.Util;
internal static class FileUtil
{
    private static Char[] _invalidFileNameChars = Path.GetInvalidFileNameChars();
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
