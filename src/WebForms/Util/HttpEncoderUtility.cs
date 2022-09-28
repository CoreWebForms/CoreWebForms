// MIT License.

using System.Diagnostics;

/*
 * Helper class for common encoding routines
 *
 * Copyright (c) 2009 Microsoft Corporation
 */

#nullable disable

namespace System.Web.Util;
internal static class HttpEncoderUtility
{

    public static int HexToInt(char h)
    {
        return (h >= '0' && h <= '9') ? h - '0' :
        (h >= 'a' && h <= 'f') ? h - 'a' + 10 :
        (h >= 'A' && h <= 'F') ? h - 'A' + 10 :
        -1;
    }

    public static char IntToHex(int n)
    {
        Debug.Assert(n < 0x10);

        return n <= 9 ? (char)(n + (int)'0') : (char)(n - 10 + (int)'a');
    }

    // Set of safe chars, from RFC 1738.4 minus '+'
    public static bool IsUrlSafeChar(char ch)
    {
        if ((ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z') || (ch >= '0' && ch <= '9'))
        {
            return true;
        }

        switch (ch)
        {
            case '-':
            case '_':
            case '.':
            case '!':
            case '*':
            case '(':
            case ')':
                return true;
        }

        return false;
    }

    //  Helper to encode spaces only
    internal static string UrlEncodeSpaces(string str)
    {
        if (str != null && str.Contains(' '))
        {
            str = str.Replace(" ", "%20");
        }

        return str;
    }

}
