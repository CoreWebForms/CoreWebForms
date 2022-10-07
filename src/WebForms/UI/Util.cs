// MIT License.

using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
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
    public static string CreateFilteredName(string deviceName, string name)
    {
        if (deviceName.Length > 0)
        {
            return deviceName + DeviceFilterSeparator + name;
        }
        return name;
    }

    internal static string GetNonEmptyFullClassNameAttribute(string name, string value,
       ref string ns)
    {

        value = GetNonEmptyNoSpaceAttribute(name, value);

        // The value can be of the form NS1.NS2.MyClassName.  Split it into its parts.
        string[] parts = value.Split('.');

        // Check that all the parts are valid identifiers
        foreach (string part in parts)
        {
            if (!System.CodeDom.Compiler.CodeGenerator.IsValidLanguageIndependentIdentifier(part))
            {
                throw new HttpException(
                    SR.GetString(SR.Invalid_attribute_value, value, name));
            }
        }

        // If there is a namespace, return it
        if (parts.Length > 1)
        {
            ns = String.Join(".", parts, 0, parts.Length - 1);
        }

        // Return the class name (which is the last part)
        return parts[parts.Length - 1];
    }

    internal static string GetAndRemoveNonEmptyNoSpaceAttribute(IDictionary directives,
       string key)
    {
        return GetAndRemoveNonEmptyNoSpaceAttribute(directives, key, false /*required*/);
    }

    internal static void CheckUnknownDirectiveAttributes(string directiveName, IDictionary directive)
    {

        CheckUnknownDirectiveAttributes(directiveName, directive, SR.Attr_not_supported_in_directive);
    }

    internal static void CheckUnknownDirectiveAttributes(string directiveName, IDictionary directive,
        string resourceKey)
    {

        // If there are some attributes left, fail
        if (directive.Count > 0)
        {
            throw new HttpException(
                SR.GetString(resourceKey,
                    FirstDictionaryKey(directive), directiveName));
        }
    }
    private static string FirstDictionaryKey(IDictionary dict)
    {
        IDictionaryEnumerator e = dict.GetEnumerator();
        e.MoveNext();
        return (string)e.Key;
    }

    internal static string GetAndRemoveRequiredAttribute(IDictionary directives, string key)
    {
        return GetAndRemoveNonEmptyAttribute(directives, key, true /*required*/);
    }

    internal /*public*/ static int LineCount(string text, int offset, int newoffset)
    {
        Debug.Assert(offset <= newoffset);

        int linecount = 0;

        while (offset < newoffset)
        {
            if (text[offset] == '\r' || (text[offset] == '\n' && (offset == 0 || text[offset - 1] != '\r')))
            {
                linecount++;
            }

            offset++;
        }

        return linecount;
    }

    internal static string GetAndRemoveNonEmptyIdentifierAttribute(IDictionary directives,
      string key, bool required)
    {

        string val = GetAndRemoveNonEmptyNoSpaceAttribute(directives, key, required);

        if (val == null)
        {
            return null;
        }

        return GetNonEmptyIdentifierAttribute(key, val);
    }

    internal static string GetAndRemoveNonEmptyAttribute(IDictionary directives, string key)
    {
        return GetAndRemoveNonEmptyAttribute(directives, key, false /*required*/);
    }

    internal static string GetNonEmptyIdentifierAttribute(string name, string value)
    {
        value = GetNonEmptyNoSpaceAttribute(name, value);

        if (!System.CodeDom.Compiler.CodeGenerator.IsValidLanguageIndependentIdentifier(value))
        {
            throw new HttpException(
                SR.GetString(SR.Invalid_attribute_value, value, name));
        }

        return value;
    }

    internal static string GetAndRemoveNonEmptyNoSpaceAttribute(IDictionary directives,
     string key, bool required)
    {

        string val = Util.GetAndRemoveNonEmptyAttribute(directives, key, required);

        if (val == null)
        {
            return null;
        }

        return GetNonEmptyNoSpaceAttribute(key, val);
    }

    internal static string GetNonEmptyNoSpaceAttribute(string name, string value)
    {
        value = GetNonEmptyAttribute(name, value);
        return GetNoSpaceAttribute(name, value);
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

    // Return the value, after checking that it doesn't contain spaces
    internal static string GetNoSpaceAttribute(string name, string value)
    {
        if (ContainsWhiteSpace(value))
        {
            throw new HttpException(
                SR.GetString(SR.Space_attribute, name));
        }

        return value;
    }

    private static bool ContainsWhiteSpace(string name)
    {
        foreach (char c in name)
        {
            if (Char.IsWhiteSpace(c))
            {
                return true;
            }
        }
        return false;
    }

    internal static void CheckAssignableType(Type baseType, Type type)
    {
        if (!baseType.IsAssignableFrom(type))
        {
            throw new HttpException(
                SR.GetString(SR.Type_doesnt_inherit_from_type,
                    type.FullName, baseType.FullName));
        }
    }

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

    internal static bool GetAndRemoveBooleanAttribute(IDictionary directives,
                                                      string key, ref bool val)
    {
        string s = Util.GetAndRemove(directives, key);

        if (s == null)
        {
            return false;
        }

        val = GetBooleanAttribute(key, s);
        return true;
    }

    internal static bool IsLateBoundComClassicType(Type t)
    {
        // 
        return (String.Compare(t.FullName, "System.__ComObject", StringComparison.Ordinal) == 0);
    }

    internal static AssemblySet GetReferencedAssemblies(Assembly a)
    {

        AssemblySet referencedAssemblies = new AssemblySet();
        AssemblyName[] refs = a.GetReferencedAssemblies();

        foreach (AssemblyName aname in refs)
        {
            Assembly referencedAssembly = Assembly.Load(aname);

            // Ignore mscorlib
            if (referencedAssembly == typeof(string).Assembly)
            {
                continue;
            }

            referencedAssemblies.Add(referencedAssembly);
        }

        return referencedAssemblies;
    }

    internal static int CommaIndexInTypeName(string typeName)
    {

        // Look for the last comma
        int commaIndex = typeName.LastIndexOf(',');

        // If it doesn't have one, there is no assembly
        if (commaIndex < 0)
        {
            return -1;
        }

        // It has a comma, we need to account for the generics syntax.
        // E.g. it could be "SomeType[int,string]

        // Check for a ]
        int rightBracketIndex = typeName.LastIndexOf(']');

        // If it has one, and it's after the last comma, there is no assembly
        if (rightBracketIndex > commaIndex)
        {
            return -1;
        }

        // The comma that we want is the first one after the last ']'
        commaIndex = typeName.IndexOf(',', rightBracketIndex + 1);

        // There is an assembly
        return commaIndex;
    }

    internal static Type GetTypeFromAssemblies(IEnumerable assemblies, string typeName, bool ignoreCase)
    {
        if (assemblies == null)
        {
            return null;
        }

        Type type = null;

        foreach (Assembly assembly in assemblies)
        {
            Type t = assembly.GetType(typeName, false /*throwOnError*/, ignoreCase);

            if (t == null)
            {
                continue;
            }

            // If we had already found a different one, it's an ambiguous type reference
            if (type != null && t != type)
            {
                throw new HttpException(SR.GetString(SR.Ambiguous_type, typeName,
                    type.FullName, t.FullName));
            }

            // Keep track of it
            type = t;
        }

        return type;
    }

    internal static void CheckVirtualFileExists(VirtualPath virtualPath)
    {
        if (!virtualPath.FileExists())
        {
            throw new HttpException(
                //HttpStatus.NotFound,
                SR.GetString(SR.FileName_does_not_exist,
                    virtualPath.VirtualPathString));
        }
    }

    internal static int GetNonNegativeIntegerAttribute(string name, string value)
    {

        int ret;

        try
        {
            ret = int.Parse(value, CultureInfo.InvariantCulture);
        }
        catch
        {
            throw new HttpException(
                SR.GetString(SR.Invalid_nonnegative_integer_attribute, name));
        }

        // Make sure it's not negative
        if (ret < 0)
        {
            throw new HttpException(
                SR.GetString(SR.Invalid_nonnegative_integer_attribute, name));
        }

        return ret;
    }

    internal static bool GetBooleanAttribute(string name, string value)
    {
        try
        {
            return bool.Parse(value);
        }
        catch
        {
            throw new HttpException(
                SR.GetString(SR.Invalid_boolean_attribute, name));
        }
    }

    internal /*public*/ static String StringFromVirtualPath(VirtualPath virtualPath)
    {

        using (Stream stream = virtualPath.OpenFile())
        {
            // Create a reader on the file, and read the whole thing
            TextReader reader = Util.ReaderFromStream(stream, virtualPath);
            return reader.ReadToEnd();
        }
    }
}
