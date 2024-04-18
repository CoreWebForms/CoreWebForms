// MIT License.

using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Security.Permissions;
using System.Text;
using System.Web.Compilation;
using System.Web.UI.WebControls;
using System.Web.Util;
using Microsoft.Extensions.FileProviders;

/*
 * Implements various utility functions used by the template code
 *
 * Copyright (c) 1998 Microsoft Corporation
 */

#nullable disable

namespace System.Web.UI;
internal static class Util
{
    internal static string GetAssemblyQualifiedTypeName(Type t) {
        if (t.Assembly.GlobalAssemblyCache)
            return t.AssemblyQualifiedName;

        // For non-GAC types, t.AssemblyQualifiedName still returns a big ugly type string,
        // so return a simpler one instead with just "typename, assemblyName".
        return t.FullName + ", " + t.Assembly.GetName().Name;
    }


    /*
     * Extract a namespace and typename from a virtualPath
     * We use all but the last two chunks as the namespace
     * e.g. Aaa.Bbb.Ccc.Wsdl will use the "Aaa.Bbb" namespace, and Ccc as the type.
     * chunksToIgnore is the number of ending chunks to ignore (e.g. 1 for the extension)
     */
    internal static string GetNamespaceAndTypeNameFromVirtualPath(VirtualPath virtualPath,
        int chunksToIgnore, out string typeName) {

        // Get the file name (with no path)
        string filename = virtualPath.FileName;

        // Split it into chunks separated by '.'
        string[] chunks = filename.Split('.');

        int chunkCount = chunks.Length - chunksToIgnore;
        Debug.Assert(chunkCount >= 1);

        if (IsWhiteSpaceString(chunks[chunkCount-1])) {
            throw new HttpException(SR.GetString(SR.Unsupported_filename, filename));
        }

        typeName = MakeValidTypeNameFromString(chunks[chunkCount-1]);

        // Turn all the relevant chunks into valid namespace chunks
        for (int i=0; i<chunkCount-1; i++) {

            if (IsWhiteSpaceString(chunks[i])) {
                throw new HttpException(SR.GetString(SR.Unsupported_filename, filename));
            }

            chunks[i] = MakeValidTypeNameFromString(chunks[i]);
        }

        // Put the relevant chunks back together
        return String.Join(".", chunks, 0, chunkCount-1);
    }



    /*
     * Returns true if the type string contains an assembly specification
     */
    internal static bool TypeNameContainsAssembly(string typeName) {
        return CommaIndexInTypeName(typeName) > 0;
    }

    /*
     * Clears a file's readonly attribute if it has one
     */
    internal static void ClearReadOnlyAttribute(string path) {

        FileAttributes attribs = File.GetAttributes(path);
        if ((attribs & FileAttributes.ReadOnly) != 0) {
            File.SetAttributes(path, attribs & ~FileAttributes.ReadOnly);
        }
    }

    internal static bool IsNonEmptyDirectory(string dir) {

        // Does it exist
        if (!Directory.Exists(dir))
            return false;

        // It exists, but maybe it's empty
        try {
            string[] entries = Directory.GetFileSystemEntries(dir);
            return entries.Length > 0;
        }
        catch {
            // If it throws, assume it's non-empty
            return true;
        }
    }
    internal static void AddAssembliesToStringCollection(ICollection fromList, StringCollection toList) {

        // Nothing to do if either is null
        if (fromList == null || toList == null)
            return;

        foreach (Assembly assembly in fromList) {
            AddAssemblyToStringCollection(assembly, toList);
        }
    }
    internal static void AddAssemblyToStringCollection(Assembly assembly, StringCollection toList) {

        string assemblyPath = null;

        //Skip adding Mscorlib for versions from 4.0 as that is added by CodeDomProvider (because of CoreAssemblyFileName switch).
        // TODO: Migration
        // if (BuildManagerHost.InClientBuildManager && !MultiTargetingUtil.IsTargetFramework20 && !MultiTargetingUtil.IsTargetFramework35) {
        //     if (assembly.FullName == typeof(string).Assembly.FullName) {
        //         return;
        //     }
        // }

        if (!MultiTargetingUtil.EnableReferenceAssemblyResolution) {
            assemblyPath = Util.GetAssemblyCodeBase(assembly);

        } else {
            // TODO: Migration
            // Get the full path to the reference assembly. For framework assemblies, this will be the path
            // to the actual target reference assembly.
            // ReferenceAssemblyType referenceAssemblyType = AssemblyResolver.GetPathToReferenceAssembly(assembly, out assemblyPath);

            // If the assembly is only available in a higher framework version, skip it.
            // If the user tries to use anything from such an assembly, he should be getting errors
            // during actual csc/vbc compilation reporting that the type or method is not found.
            // if (referenceAssemblyType == ReferenceAssemblyType.FrameworkAssemblyOnlyPresentInHigherVersion) {
            //     return;
            // }
        }

        Debug.Assert(!String.IsNullOrEmpty(assemblyPath));

        // Unless it's already in the list, add it
        if (!toList.Contains(assemblyPath)) {
            toList.Add(assemblyPath);
        }
    }

    /*
     * Return a String which holds the contents of a file, or null if the file
     * doesn't exist.
     */
    internal /*public*/ static String StringFromFileIfExists(string path) {

        if (!File.Exists(path)) return null;

        return StringFromFile(path);
    }
    internal static Type GetNonPrivatePropertyType(Type classType, string propName)
    {
        PropertyInfo propInfo = null;

        BindingFlags flags = BindingFlags.Public | BindingFlags.Instance
            | BindingFlags.IgnoreCase | BindingFlags.NonPublic;

        try
        {
            propInfo = classType.GetProperty(propName, flags);
        }
        catch (AmbiguousMatchException)
        {

            // We could get an AmbiguousMatchException if the property exists on two
            // different ancestor classes (VSWhidbey 216957).  When that happens, attempt
            // a lookup on the Type itself, ignoring its ancestors.

            flags |= BindingFlags.DeclaredOnly;
            propInfo = classType.GetProperty(propName, flags);
        }

        if (propInfo == null)
        {
            return null;
        }

        // If it doesn't have a setter, ot if it's private, fail
        MethodInfo methodInfo = propInfo.GetSetMethod(true /*nonPublic*/);
        if (methodInfo == null || methodInfo.IsPrivate)
        {
            return null;
        }

        return propInfo.PropertyType;
    }

    internal /*public*/ static String StringFromFile(string path) {
        Encoding encoding = Encoding.Default;
        return StringFromFile(path, ref encoding);
    }

    /*
     * Return a String which holds the contents of a file with specific encoding.
     */
    internal /*public*/ static String StringFromFile(string path, ref Encoding encoding) {

        // Create a reader on the file.
        // Generates an exception if the file can't be opened.
        StreamReader reader = new StreamReader(path, encoding, true /*detectEncodingFromByteOrderMarks*/);

        try {
            string content = reader.ReadToEnd();
            encoding = reader.CurrentEncoding;

            return content;
        }
        finally {
            // Make sure we always close the stream
            if (reader != null)
                reader.Close();
        }
    }

    internal static void DeleteFileNoException(string path) {
        Debug.Assert(File.Exists(path), path);
        try {
            File.Delete(path);
        }
        catch { } // Ignore all exceptions
    }

    internal static void DeleteFileIfExistsNoException(string path) {
        if (File.Exists(path))
            DeleteFileNoException(path);
    }

    internal static Type GetNonPrivateFieldType(Type classType, string fieldName)
    {
        FieldInfo fieldInfo = classType.GetField(fieldName,
            BindingFlags.IgnoreCase | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

        if (fieldInfo == null || fieldInfo.IsPrivate)
        {
            return null;
        }

        return fieldInfo.FieldType;
    }

    /*
     * Return a full type name from a namespace (could be empty) and a type name
     */
    internal static string MakeFullTypeName(string ns, string typeName)
    {
        if (String.IsNullOrEmpty(ns))
        {
            return typeName;
        }

        return ns + "." + typeName;
    }

    /*
     * Return a valid type name from a string by changing any character
     * that's not a letter or a digit to an '_'.
     */
    internal static string MakeValidTypeNameFromString(string s)
    {
        StringBuilder sb = new StringBuilder();

        sb.Append("__");

        for (int i = 0; i < s.Length; i++)
        {
            // Make sure it doesn't start with a digit (ASURT 31134)
            if (i == 0 && Char.IsDigit(s[0]))
            {
                sb.Append('_');
            }

            if (Char.IsLetterOrDigit(s[i]))
            {
                sb.Append(s[i]);
            }
            else
            {
                sb.Append('_');
            }
        }

        return sb.ToString();
    }

    internal static bool IsMultiInstanceTemplateProperty(PropertyInfo pInfo)
    {
        object[] instanceAttrs = pInfo.GetCustomAttributes(typeof(TemplateInstanceAttribute), /*inherits*/ false);

        // Default value for TemplateInstanceAttribute is TemplateInstance.Multiple
        if (instanceAttrs == null || instanceAttrs.Length == 0)
        {
            return true;
        }

        return ((TemplateInstanceAttribute)instanceAttrs[0]).Instances == TemplateInstance.Multiple;
    }

    internal static long GetRecompilationHash(PagesSection ps)
    {
        HashCodeCombiner recompilationHash = new HashCodeCombiner();

        // TODO: Migration
        // NamespaceCollection namespaces;
        // TagPrefixCollection controls;
        // TagMapCollection tagMapping;
        //
        // // Combine items from Pages section
        // recompilationHash.AddObject(ps.Buffer);
        // recompilationHash.AddObject(ps.EnableViewState);
        // recompilationHash.AddObject(ps.EnableViewStateMac);
        // recompilationHash.AddObject(ps.EnableEventValidation);
        // recompilationHash.AddObject(ps.SmartNavigation);
        // recompilationHash.AddObject(ps.ValidateRequest);
        // recompilationHash.AddObject(ps.AutoEventWireup);
        // if (ps.PageBaseTypeInternal != null) {
        //     recompilationHash.AddObject(ps.PageBaseTypeInternal.FullName);
        // }
        // if (ps.UserControlBaseTypeInternal != null) {
        //     recompilationHash.AddObject(ps.UserControlBaseTypeInternal.FullName);
        // }
        // if (ps.PageParserFilterTypeInternal != null) {
        //     recompilationHash.AddObject(ps.PageParserFilterTypeInternal.FullName);
        // }
        // recompilationHash.AddObject(ps.MasterPageFile);
        // recompilationHash.AddObject(ps.Theme);
        // recompilationHash.AddObject(ps.StyleSheetTheme);
        // recompilationHash.AddObject(ps.EnableSessionState);
        // recompilationHash.AddObject(ps.CompilationMode);
        // recompilationHash.AddObject(ps.MaxPageStateFieldLength);
        // recompilationHash.AddObject(ps.ViewStateEncryptionMode);
        // recompilationHash.AddObject(ps.MaintainScrollPositionOnPostBack);
        //
        // // Combine items from Namespaces collection
        // namespaces = ps.Namespaces;
        //
        // recompilationHash.AddObject(namespaces.AutoImportVBNamespace);
        // if (namespaces.Count == 0) {
        //     recompilationHash.AddObject("__clearnamespaces");
        // }
        // else {
        //     foreach (NamespaceInfo ni in namespaces) {
        //         recompilationHash.AddObject(ni.Namespace);
        //     }
        // }
        //
        // // Combine items from the Controls collection
        // controls = ps.Controls;
        //
        // if (controls.Count == 0) {
        //     recompilationHash.AddObject("__clearcontrols");
        // }
        // else {
        //     foreach (TagPrefixInfo tpi in controls) {
        //         recompilationHash.AddObject(tpi.TagPrefix);
        //
        //         if (tpi.TagName != null && tpi.TagName.Length != 0) {
        //             recompilationHash.AddObject(tpi.TagName);
        //             recompilationHash.AddObject(tpi.Source);
        //         }
        //         else {
        //             recompilationHash.AddObject(tpi.Namespace);
        //             recompilationHash.AddObject(tpi.Assembly);
        //         }
        //     }
        // }
        //
        // // Combine items from the TagMapping Collection
        // tagMapping = ps.TagMapping;
        //
        // if (tagMapping.Count == 0) {
        //     recompilationHash.AddObject("__cleartagmapping");
        // }
        // else {
        //     foreach (TagMapInfo tmi in tagMapping) {
        //         recompilationHash.AddObject(tmi.TagType);
        //         recompilationHash.AddObject(tmi.MappedTagType);
        //     }
        // }

        return recompilationHash.CombinedHash;
    }

    internal static bool IsFalseString(string s)
    {
        return s != null && (StringUtil.EqualsIgnoreCase(s, "false"));
    }
    internal static bool IsTrueString(string s)
    {
        return s != null && (StringUtil.EqualsIgnoreCase(s, "true"));
    }


    internal static string GetAssemblyShortName(Assembly a) {

        // Getting the short name is always safe, so Assert to get it (VSWhidbey 491895)
        // InternalSecurityPermissions.Unrestricted.Assert();

        return a.GetName().Name;
    }

    internal static string MakeValidFileName(string fileName) {

        // TODO: Migration
        // // If it's already valid, nothing to do
        // if (IsValidFileName(fileName))
        //     return fileName;
        //
        // // Replace all the invalid chars by '_'
        // for (int i = 0; i < invalidFileNameChars.Length; ++i)  {
        //     fileName = fileName.Replace(invalidFileNameChars[i], '_');
        // }
        //
        // // Shoud always be valid now
        // Debug.Assert(IsValidFileName(fileName));

        return fileName;
    }

    internal static void CheckThemeAttribute(string themeName)
    {
        if (themeName.Length > 0)
        {
            if (!FileUtil.IsValidDirectoryName(themeName))
            {
                throw new HttpException(SR.GetString(SR.Page_theme_invalid_name, themeName));
            }

            if (!ThemeExists(themeName))
            {
                throw new HttpException(SR.GetString(SR.Page_theme_not_found, themeName));
            }
        }
    }

    internal static bool ThemeExists(string themeName)
    {
#if PORT_THEMES
        VirtualPath virtualDir = ThemeDirectoryCompiler.GetAppThemeVirtualDir(themeName);
        if (!VirtualDirectoryExistsWithAssert(virtualDir))
        {
            virtualDir = ThemeDirectoryCompiler.GetGlobalThemeVirtualDir(themeName);
            if (!VirtualDirectoryExistsWithAssert(virtualDir))
            {
                return false;
            }
        }

        return true;
#endif
        throw new NotImplementedException("Themes are not available");
    }

    private static bool VirtualDirectoryExistsWithAssert(VirtualPath virtualDir) {
        try {
            return virtualDir.DirectoryExists();
        }
        catch {
            return false;
        }
    }

    internal static bool GetAndRemovePositiveIntegerAttribute(IDictionary directives,
                                                             string key, ref int val)
    {
        string s = Util.GetAndRemove(directives, key);

        if (s == null)
        {
            return false;
        }

        try
        {
            val = int.Parse(s, CultureInfo.InvariantCulture);
        }
        catch
        {
            throw new HttpException(
                SR.GetString(SR.Invalid_positive_integer_attribute, key));
        }

        // Make sure it's positive
        if (val <= 0)
        {
            throw new HttpException(
                SR.GetString(SR.Invalid_positive_integer_attribute, key));
        }

        return true;
    }

    internal static VirtualPath GetScriptLocation() {
        // Todo: Migration
        // prepare script include
        // Dev10 Bug564221: we need to detect if app level web.config overwrites the root web.conigf
        // string location = (string) RuntimeConfig.GetAppConfig().WebControls["clientScriptsLocation"];
        string location = "/aspnet_client/{0}/{1}/";
        // If there is a formatter, as there will be for the default machine.config, insert the assembly name and version.
        if (location.IndexOf("{0}", StringComparison.Ordinal) >= 0) {
            string assembly = "system_web";

            // Todo: Migration
            // QFE number is not included in client path
            string version = VersionInfo.SystemWebVersion.Substring(0, VersionInfo.SystemWebVersion.LastIndexOf('.')).Replace('.', '_');
            location = String.Format(CultureInfo.InvariantCulture, location, assembly, version);
        }

        return VirtualPath.Create(location);
    }

    internal static object GetAndRemoveEnumAttribute(IDictionary directives, Type enumType,
                                                  string key)
    {
        string s = Util.GetAndRemove(directives, key);

        if (s == null)
        {
            return null;
        }

        return GetEnumAttribute(key, s, enumType);
    }

    internal static object GetEnumAttribute(string name, string value, Type enumType)
    {
        return GetEnumAttribute(name, value, enumType, false);
    }

    internal static object GetEnumAttribute(string name, string value, Type enumType, bool allowMultiple)
    {
        object val;

        try
        {
            // Don't allow numbers to be specified (ASURT 71851)
            // Also, don't allow several values (e.g. "red,blue")
            if (Char.IsDigit(value[0]) || value[0] == '-' || ((!allowMultiple) && (value.Contains(','))))
            {
                throw new FormatException(SR.GetString(SR.EnumAttributeInvalidString, value, name, enumType.FullName));
            }

            val = Enum.Parse(enumType, value, true /*ignoreCase*/);
        }
        catch
        {
            string names = null;
            foreach (string n in Enum.GetNames(enumType))
            {
                if (names == null)
                {
                    names = n;
                }
                else
                {
                    names += ", " + n;
                }
            }
            throw new HttpException(
                SR.GetString(SR.Invalid_enum_attribute, name, names));
        }

        return val;
    }

    internal static string GetStringFromBool(bool flag)
    {
        return flag ? "true" : "false";
    }

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

    internal static void CheckVirtualFileExists(VirtualPath virtualPath, IFileProvider fileProvider)
    {
        if (!virtualPath.FileExists(fileProvider))
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

    internal /*public*/ static String StringFromVirtualPath(VirtualPath virtualPath, IFileProvider fileProvider)
    {

        using (Stream stream = virtualPath.OpenFile(fileProvider))
        {
            // Create a reader on the file, and read the whole thing
            TextReader reader = Util.ReaderFromStream(stream, virtualPath);
            return reader.ReadToEnd();
        }
    }

    internal static string GetClientValidatedPostback(Control control, string validationGroup) {
        return GetClientValidatedPostback(control, validationGroup, string.Empty);
    }

    internal static string GetClientValidatedPostback(Control control,
        string validationGroup,
        string argument) {
        string postbackReference = control.Page.ClientScript.GetPostBackEventReference(control, argument, true);
        return GetClientValidateEvent(validationGroup) + postbackReference;
    }

    internal static string GetClientValidateEvent(string validationGroup) {
        if (validationGroup == null) {
            validationGroup = String.Empty;
        }
        return "if (typeof(Page_ClientValidate) == 'function') Page_ClientValidate('" +
               validationGroup +
               "'); ";
    }

    internal static bool TypeNameContainsAssembly(string typeName)
    {
        return CommaIndexInTypeName(typeName) > 0;
    }

    internal static void RemoveOrRenameFile(string filename) {
        FileInfo fi = new FileInfo(filename);
        RemoveOrRenameFile(fi);
    }


    /*
     * If the file doesn't exist, do nothing.  If it does try to delete it if possible.
     * If that fails, rename it with by appending a .delete extension to it
     */
    internal static bool RemoveOrRenameFile(FileInfo f) {
        try {
            // First, just try to delete the file
            f.Delete();

            // It was successfully deleted, so return true
            return true;
        }
        catch {

            try {
                // If the delete failed, rename it to ".delete"
                // Don't do that if it already has the delete extension
                if (f.Extension != ".delete") {

                    // include a unique token as part of the new name, to avoid
                    // conflicts with previous renames (VSWhidbey 79996)
                    string uniqueToken = DateTime.Now.Ticks.GetHashCode().ToString("x", CultureInfo.InvariantCulture);
                    string newName = f.FullName + "." + uniqueToken + ".delete";
                    f.MoveTo(newName);
                }
            }
            catch {
                // Ignore all exceptions
            }
        }

        // Return false because we couldn't delete it, and had to rename it
        return false;
    }

    /*
     * Return an assembly name from the name of an assembly dll.
     * Basically, it strips the extension.
     */
    internal static string GetAssemblyNameFromFileName(string fileName) {
        // Strip the .dll extension if any
        if (StringUtil.EqualsIgnoreCase(Path.GetExtension(fileName), ".dll"))
            return fileName.Substring(0, fileName.Length-4);

        return fileName;
    }

    /*
     * Return the culture name for a file (e.g. "fr" or "fr-fr").
     * If no culture applies, return null.
     */
    internal static string GetCultureName(string virtualPath) {

        if (virtualPath == null) return null;

        // By default, extract the culture name from the file name (e.g. "foo.fr-fr.resx")

        string fileNameNoExt = Path.GetFileNameWithoutExtension(virtualPath);

        // If virtualPath is not a file, ie. above statement returns null, simply return null;
        if (fileNameNoExt == null)
            return null;

        // If there a dot left
        int dotIndex = fileNameNoExt.LastIndexOf('.');

        if (dotIndex < 0) return null;

        string cultureName = fileNameNoExt.Substring(dotIndex+1);

        // If it doesn't look like a culture name (e.g. "fr" or "fr-fr"), return null
        if (!IsCultureName(cultureName))
            return null;

        return cultureName;
    }

    /*
     * Checks whether the passed in string is a valid culture name.
     */
    internal static bool IsCultureName(string s)
    {
        // Todo : Migration
        return true;
    }

    /*
     * Get the path to the (shadow copied) DLL behind an assembly
     */
    internal static string GetAssemblyCodeBase(Assembly assembly) {

        string location = assembly.Location;
        if (String.IsNullOrEmpty(location))
            return null;

        // Get the path to the assembly (from the cache if it got shadow copied)
        return location;
    }

    private static char[] invalidFileNameChars = new char[] { '/', '\\', '?', '*', ':' } ;
    internal static bool IsValidFileName(string fileName) {

        // Check for the special names "." and ".."
        if (fileName == "." || fileName == "..")
            return false;

        // Check for invalid characters
        if (fileName.IndexOfAny(invalidFileNameChars) >= 0)
            return false;

        return true;
    }
}
