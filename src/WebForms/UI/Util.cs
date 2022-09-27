// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Text;

/*
 * Implements various utility functions used by the template code
 *
 * Copyright (c) 1998 Microsoft Corporation
 */

#nullable disable

namespace System.Web.UI;
internal static class Util
{
    internal static bool CanConvertToFrom(TypeConverter converter, Type type)
    {
        return (converter != null && converter.CanConvertTo(type) &&
                converter.CanConvertFrom(type) && !(converter is ReferenceConverter));
    }

    internal static string QuoteJScriptString(string value)
    {
        return QuoteJScriptString(value, false);
    }

    internal static string MergeScript(string firstScript, string secondScript)
    {
        Debug.Assert(!string.IsNullOrEmpty(secondScript));

        if (!string.IsNullOrEmpty(firstScript))
        {
            // 
            return firstScript + secondScript;
        }
        else
        {
            return secondScript.TrimStart().StartsWith(ClientScriptManager.JscriptPrefix, StringComparison.Ordinal)
                ? secondScript
                : ClientScriptManager.JscriptPrefix + secondScript;
        }
    }

    internal static string QuoteJScriptString(string value, bool forUrl)
    {
        StringBuilder b = null;

        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        int startIndex = 0;
        int count = 0;
        for (int i = 0; i < value.Length; i++)
        {
            switch (value[i])
            {
                case '\r':
                    if (b == null)
                    {
                        b = new StringBuilder(value.Length + 5);
                    }

                    if (count > 0)
                    {
                        b.Append(value, startIndex, count);
                    }

                    b.Append("\\r");

                    startIndex = i + 1;
                    count = 0;
                    break;
                case '\t':
                    if (b == null)
                    {
                        b = new StringBuilder(value.Length + 5);
                    }

                    if (count > 0)
                    {
                        b.Append(value, startIndex, count);
                    }

                    b.Append("\\t");

                    startIndex = i + 1;
                    count = 0;
                    break;
                case '\"':
                    if (b == null)
                    {
                        b = new StringBuilder(value.Length + 5);
                    }

                    if (count > 0)
                    {
                        b.Append(value, startIndex, count);
                    }

                    b.Append("\\\"");

                    startIndex = i + 1;
                    count = 0;
                    break;
                case '\'':
                    if (b == null)
                    {
                        b = new StringBuilder(value.Length + 5);
                    }

                    if (count > 0)
                    {
                        b.Append(value, startIndex, count);
                    }

                    b.Append("\\\'");

                    startIndex = i + 1;
                    count = 0;
                    break;
                case '\\':
                    if (b == null)
                    {
                        b = new StringBuilder(value.Length + 5);
                    }

                    if (count > 0)
                    {
                        b.Append(value, startIndex, count);
                    }

                    b.Append("\\\\");

                    startIndex = i + 1;
                    count = 0;
                    break;
                case '\n':
                    if (b == null)
                    {
                        b = new StringBuilder(value.Length + 5);
                    }

                    if (count > 0)
                    {
                        b.Append(value, startIndex, count);
                    }

                    b.Append("\\n");

                    startIndex = i + 1;
                    count = 0;
                    break;
                case '%':
                    if (forUrl)
                    {
                        if (b == null)
                        {
                            b = new StringBuilder(value.Length + 6);
                        }
                        if (count > 0)
                        {
                            b.Append(value, startIndex, count);
                        }
                        b.Append("%25");

                        startIndex = i + 1;
                        count = 0;
                        break;
                    }
                    goto default;
                default:
                    count++;
                    break;
            }
        }

        if (b == null)
        {
            return value;
        }

        if (count > 0)
        {
            b.Append(value, startIndex, count);
        }

        return b.ToString();
    }

    internal static object InvokeMethod(
                                       MethodInfo methodInfo,
                                       object obj,
                                       object[] parameters)
    {
        try
        {
            return methodInfo.Invoke(obj, parameters);
        }
        catch (TargetInvocationException e)
        {
            throw e.InnerException;
        }
    }
    internal static string EnsureEndWithSemiColon(string value)
    {
        if (value != null)
        {
            int length = value.Length;
            if (length > 0 && value[length - 1] != ';')
            {
                return value + ";";
            }
        }
        return value;
    }

    internal static bool IsUserAllowedToPath(HttpContext context, VirtualPath previousPagePath)
    {
        return true;
    }

    internal static int FirstNonWhiteSpaceIndex(string s)
    {
        for (int i = 0; i < s.Length; i++)
        {
            if (!Char.IsWhiteSpace(s[i]))
            {
                return i;
            }
        }

        return -1;
    }
}
