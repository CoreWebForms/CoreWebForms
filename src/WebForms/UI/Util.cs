// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Web.Util;

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

    /*
     * Return an assembly name from the name of an assembly dll.
     * Basically, it strips the extension.
     */
    internal static string GetAssemblyNameFromFileName(string fileName)
    {
        // Strip the .dll extension if any
        if (StringUtil.EqualsIgnoreCase(Path.GetExtension(fileName), ".dll"))
            return fileName.Substring(0, fileName.Length - 4);

        return fileName;
    }

    /*
     * Returns true if the type string contains an assembly specification
     */
    internal static bool TypeNameContainsAssembly(string typeName)
    {
        return CommaIndexInTypeName(typeName) > 0;
    }

    /*
     * Look for a type by name in a collection of assemblies.  If it exists in multiple assemblies,
     * throw an error.
     */
    internal static Type GetTypeFromAssemblies(Collections.IEnumerable assemblies, string typeName, bool ignoreCase)
    {
        if (assemblies == null)
            return null;

        Type type = null;

        foreach (Assembly assembly in assemblies)
        {
            Type t = assembly.GetType(typeName, false /*throwOnError*/, ignoreCase);

            if (t == null)
                continue;

            // If we had already found a different one, it's an ambiguous type reference
            if (type != null && t != type)
            {
                throw new HttpException(SR.GetString(SR.Ambiguous_type, typeName,
                    GetAssemblySafePathFromType(type), GetAssemblySafePathFromType(t)));
            }

            // Keep track of it
            type = t;
        }

        return type;
    }


    internal static string GetSafePath(string path)
    {
        if (String.IsNullOrEmpty(path))
            return path;

        //try
        //{
        //    if (HasPathDiscoveryPermission(path)) // could throw on bad filenames
        //        return path;
        //}
        //catch
        //{
        //}

        return Path.GetFileName(path);
    }

    /*
     * Return the full path (non shadow copied) to the assembly that
     * the given type lives in.
     */
    internal static string GetAssemblyPathFromType(Type t)
    {
        return Util.FilePathFromFileUrl(t.Assembly.EscapedCodeBase);
    }

    /*
     * Return a standard path from a file:// url
     */
    internal static string FilePathFromFileUrl(string url)
    {

        // 
        Uri uri = new Uri(url);
        string path = uri.LocalPath;
        return HttpUtility.UrlDecode(path);
    }

    /*
     * Same as GetAssemblyPathFromType, but with path safety check
     */
    internal static string GetAssemblySafePathFromType(Type t)
    {
        return GetSafePath(GetAssemblyPathFromType(t));
    }

    /*
     * Returns the index of the comma separating the type from the assembly, or
     * -1 of there is no assembly
     */
    internal static int CommaIndexInTypeName(string typeName)
    {

        // Look for the last comma
        int commaIndex = typeName.LastIndexOf(',');

        // If it doesn't have one, there is no assembly
        if (commaIndex < 0)
            return -1;

        // It has a comma, we need to account for the generics syntax.
        // E.g. it could be "SomeType[int,string]

        // Check for a ]
        int rightBracketIndex = typeName.LastIndexOf(']');

        // If it has one, and it's after the last comma, there is no assembly
        if (rightBracketIndex > commaIndex)
            return -1;

        // The comma that we want is the first one after the last ']'
        commaIndex = typeName.IndexOf(',', rightBracketIndex + 1);

        // There is an assembly
        return commaIndex;
    }
}
