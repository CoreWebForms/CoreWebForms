// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;

namespace Microsoft.AspNetCore.SystemWebAdapters.UI.PageParser.Syntax;

internal static class StringExtensions
{
    [DebuggerStepThrough]
    public static bool IsNullOrEmpty(this string s)
    {
        return string.IsNullOrEmpty(s);
    }

    public static bool IsNullOrWhiteSpace(this string s)
    {
        return string.IsNullOrWhiteSpace(s);
    }

    public static bool EqualsNoCase(this string first, string second)
    {
        return string.Equals(first, second, StringComparison.OrdinalIgnoreCase);
    }

    public static bool StartsWithNoCase(this string first, string second)
    {
        return first.StartsWith(second, StringComparison.OrdinalIgnoreCase);
    }

    public static bool EndsWithNoCase(this string first, string second)
    {
        return first.EndsWith(second, StringComparison.OrdinalIgnoreCase);
    }
}
