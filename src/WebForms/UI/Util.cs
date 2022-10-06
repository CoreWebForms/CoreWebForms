// MIT License.

using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Web.UI.WebControls;
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
    internal static bool IsWhiteSpaceString(string s)
    {
        return (s.Trim().Length == 0);
    }

    internal static void CopyBaseAttributesToInnerControl(WebControl control, WebControl child)
    {
        short oldTab = control.TabIndex;
        string oldAccess = control.AccessKey;
        try
        {
            control.AccessKey = String.Empty;
            control.TabIndex = 0;
            child.CopyBaseAttributes(control);
        }
        finally
        {
            control.TabIndex = oldTab;
            control.AccessKey = oldAccess;
        }
    }

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

    internal static void WriteOnClickAttribute(HtmlTextWriter writer,
                                              HtmlControls.HtmlControl control,
                                              bool submitsAutomatically,
                                              bool submitsProgramatically,
                                              bool causesValidation,
                                              string validationGroup)
        => throw new NotImplementedException();

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

    internal /*public*/ static StreamReader ReaderFromFile(string filename, VirtualPath configPath)
    {

        StreamReader reader;

        // Check if a file encoding is specified in the config
        Encoding fileEncoding = Encoding.Default;
        if (configPath != null)
        {
            fileEncoding = GetEncodingFromConfigPath(configPath);
        }

        try
        {
            // Create a reader on the file, using the encoding
            // Throws an exception if the file can't be opened.
            reader = new StreamReader(filename, fileEncoding,
                true /*detectEncodingFromByteOrderMarks*/, 4096);
        }
        catch (UnauthorizedAccessException)
        {
            // AccessException might mean two very different things: it could be a real
            // access problem, or it could be that it's actually a directory.

            // It's a directory: give a specific error.
            if (Directory.Exists(filename))
            {
                throw new HttpException(
                    SR.GetString(SR.Unexpected_Directory, filename));
            }

            // It's a real access problem, so just rethrow it
            throw;
        }

        return reader;
    }

    internal /*public*/ static StreamReader ReaderFromStream(Stream stream, VirtualPath configPath)
    {

        // Check if a file encoding is specified in the config
        Encoding fileEncoding = GetEncodingFromConfigPath(configPath);

        // Create a reader on the file, using the encoding
        return new StreamReader(stream, fileEncoding,
            true /*detectEncodingFromByteOrderMarks*/, 4096);
    }

    internal static Encoding GetEncodingFromConfigPath(VirtualPath configPath)
    {
#if PORT_CONFIG
        Debug.Assert(configPath != null, "configPath != null");

        // Check if a file encoding is specified in the config
        Encoding fileEncoding = null;
        GlobalizationSection globConfig = RuntimeConfig.GetConfig(configPath).Globalization;
        fileEncoding = globConfig.FileEncoding;

        // If not, use the default encoding
        if (fileEncoding == null)
            fileEncoding = Encoding.Default;

        return fileEncoding;
#else
        return Encoding.UTF8;
#endif
    }

    internal static VirtualPath GetAndRemoveVirtualPathAttribute(IDictionary directives, string key)
    {
        return GetAndRemoveVirtualPathAttribute(directives, key, false /*required*/);
    }

    internal static VirtualPath GetAndRemoveVirtualPathAttribute(IDictionary directives, string key, bool required)
    {

        string val = GetAndRemoveNonEmptyAttribute(directives, key, required);
        if (val == null)
        {
            return null;
        }

        return VirtualPath.Create(val);
    }

    internal static string GetAndRemoveNonEmptyAttribute(IDictionary directives, string key, bool required)
    {
        string val = Util.GetAndRemove(directives, key);

        if (val == null)
        {
            if (required)
            {
                throw new HttpException(SR.GetString(SR.Missing_attr, key));
            }

            return null;
        }

        return GetNonEmptyAttribute(key, val);
    }

    private static string GetAndRemove(IDictionary dict, string key)
    {
        string val = (string)dict[key];
        if (val != null)
        {
            dict.Remove(key);
            val = val.Trim();
        }
        return val;
    }

    internal static string GetNonEmptyAttribute(string name, string value)
    {

        value = value.Trim();

        if (value.Length == 0)
        {
            throw new HttpException(
                SR.GetString(SR.Empty_attribute, name));
        }

        return value;
    }

    internal const char DeviceFilterSeparator = ':';
    internal const string XmlnsAttribute = "xmlns:";
    public static string ParsePropertyDeviceFilter(string input, out string propName)
    {
        string deviceName = String.Empty;

        // If the string has no device filter, the whole string is the property name
        if (input.IndexOf(DeviceFilterSeparator) < 0)
        {
            propName = input;
        }
        // Don't treat xmlns as filters, this needs to be treated differently.
        // VSWhidbey 495125
        else if (StringUtil.StringStartsWithIgnoreCase(input, XmlnsAttribute))
        {
            propName = input;
        }
        else
        {
            // There is a filter: parse it out
            string[] tmp = input.Split(DeviceFilterSeparator);

            if (tmp.Length > 2)
            {
                throw new HttpException(
                    SR.GetString(SR.Too_many_filters, input));
            }

            if (MTConfigUtil.GetPagesConfig().IgnoreDeviceFilters[tmp[0]] != null)
            {
                propName = input;
            }
            else
            {
                deviceName = tmp[0];
                propName = tmp[1];
            }
        }

        return deviceName;
    }

}
