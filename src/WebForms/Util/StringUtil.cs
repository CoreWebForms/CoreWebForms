// MIT License.

using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

#nullable disable

namespace System.Web.Util;
/*
 * Various string handling utilities
 */
internal static class StringUtil
{
    internal static string CheckAndTrimString(string paramValue, string paramName)
    {
        return CheckAndTrimString(paramValue, paramName, true);
    }

    internal static string CheckAndTrimString(string paramValue, string paramName, bool throwIfNull)
    {
        return CheckAndTrimString(paramValue, paramName, throwIfNull, -1);
    }

    internal static string CheckAndTrimString(string paramValue, string paramName,
                                              bool throwIfNull, int lengthToCheck)
    {
        if (paramValue == null)
        {
            return throwIfNull ? throw new ArgumentNullException(paramName) : null;
        }
        string trimmedValue = paramValue.Trim();
        if (trimmedValue.Length == 0)
        {
            throw new ArgumentException(
                SR.GetString(SR.PersonalizationProviderHelper_TrimmedEmptyString,
                                                 paramName));
        }
        return lengthToCheck > -1 && trimmedValue.Length > lengthToCheck
            ? throw new ArgumentException(
                SR.GetString(SR.StringUtil_Trimmed_String_Exceed_Maximum_Length,
                                                 paramValue, paramName, lengthToCheck.ToString(CultureInfo.InvariantCulture)))
            : trimmedValue;
    }

    internal static bool Equals(string s1, string s2)
    {
        if (s1 == s2)
        {
            return true;
        }

        return string.IsNullOrEmpty(s1) && string.IsNullOrEmpty(s2);
    }

    internal static bool EqualsIgnoreCase(string s1, string s2)
    {
        if (string.IsNullOrEmpty(s1) && string.IsNullOrEmpty(s2))
        {
            return true;
        }
        if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2))
        {
            return false;
        }
        return s2.Length != s1.Length ? false : 0 == string.Compare(s1, 0, s2, 0, s2.Length, StringComparison.OrdinalIgnoreCase);
    }

    internal static bool EqualsIgnoreCase(string s1, int index1, string s2, int index2, int length)
    {
        return string.Compare(s1, index1, s2, index2, length, StringComparison.OrdinalIgnoreCase) == 0;
    }

    internal static string StringFromCharPtr(IntPtr ip, int length)
    {
        return Marshal.PtrToStringAnsi(ip, length);
    }

    /*
     * Determines if the string ends with the specified character.
     * Fast, non-culture aware.
     */
    internal static bool StringEndsWith(string s, char c)
    {
        int len = s.Length;
        return len != 0 && s[len - 1] == c;
    }

    /*
     * Determines if the first string ends with the second string, ignoring case.
     * Fast, non-culture aware.
     */
    internal static bool StringEndsWithIgnoreCase(string s1, string s2)
    {
        int offset = s1.Length - s2.Length;
        return offset < 0 ? false : 0 == string.Compare(s1, offset, s2, 0, s2.Length, StringComparison.OrdinalIgnoreCase);
    }

    /*
     * Determines if the string starts with the specified character.
     * Fast, non-culture aware.
     */
    internal static bool StringStartsWith(string s, char c)
    {
        return s.Length != 0 && (s[0] == c);
    }

    /*
     * Determines if the first string starts with the second string.
     * Fast, non-culture aware.
     */
    internal static bool StringStartsWith(string s1, string s2)
    {
        return s1.StartsWith(s2);
    }

    /*
     * Determines if the first string starts with the second string, ignoring case.
     * Fast, non-culture aware.
     */
    internal static bool StringStartsWithIgnoreCase(string s1, string s2)
    {
        if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2))
        {
            return false;
        }

        return s2.Length > s1.Length ? false : 0 == string.Compare(s1, 0, s2, 0, s2.Length, StringComparison.OrdinalIgnoreCase);
    }

    internal static bool StringArrayEquals(string[] a, string[] b)
    {
        if (a == null != (b == null))
        {
            return false;
        }

        if (a == null)
        {
            return true;
        }

        int n = a.Length;
        if (n != b.Length)
        {
            return false;
        }

        for (int i = 0; i < n; i++)
        {
            if (a[i] != b[i])
            {
                return false;
            }
        }

        return true;
    }

    // This is copied from String.GetHashCode.  We want our own copy, because the result of
    // String.GetHashCode is not guaranteed to be the same from build to build.  But we want a
    // stable hash, since we persist things to disk (e.g. precomp scenario).  VSWhidbey 399279.
    internal static int GetStringHashCode(string s)
    {
        var src = s;

        int hash1 = (5381 << 16) + 5381;
        int hash2 = hash1;

        // 32bit machines.
        var pint = src.AsSpan();
        int len = s.Length;
        while (len > 0)
        {
            hash1 = ((hash1 << 5) + hash1 + (hash1 >> 27)) ^ pint[0];
            if (len <= 2)
            {
                break;
            }
            hash2 = ((hash2 << 5) + hash2 + (hash2 >> 27)) ^ pint[1];
            pint = pint[2..];
            len -= 4;
        }
        return hash1 + (hash2 * 1566083941);
    }

    internal static int GetNonRandomizedHashCode(string s, bool ignoreCase = false)
    {
        if (ignoreCase)
        {
            s = s.ToLower(CultureInfo.InvariantCulture);
        }

        // Use our stable hash algorithm implementation
        return GetStringHashCode(s);
    }

    internal static int GetNullTerminatedByteArray(Encoding enc, string s, out byte[] bytes)
    {
        bytes = null;
        if (s == null)
        {
            return 0;
        }

        // Encoding.GetMaxByteCount is faster than GetByteCount, but will probably allocate more
        // memory than needed.  Working with small short-lived strings here, so that's probably ok.
        bytes = new byte[enc.GetMaxByteCount(s.Length) + 1];
        return enc.GetBytes(s, 0, s.Length, bytes, 0);
    }


    internal static string[] ObjectArrayToStringArray(object[] objectArray) {
        String[] stringKeys = new String[objectArray.Length];
        objectArray.CopyTo(stringKeys, 0);
        return stringKeys;
    }
}
