// MIT License.

using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Security;
using System.Web.Compilation;
using BuildProvider = System.Web.Compilation.BuildProvider;

namespace System.Web.UI;

internal class BuildManagerHost
{
    public static bool InClientBuildManager { get; internal set; }
}

internal class ResourceExpressionBuilder
{
    internal static IResourceProvider GetLocalResourceProvider(VirtualPath virtualPath)
    {
        throw new NotImplementedException("GetLocalResourceProvider");
    }

    internal static object ParseExpression(string fullResourceKey)
    {
        throw new NotImplementedException("ParseExpression");
    }

    internal static object GetGlobalResourceObject(string classKey, string resourceKey)
    {
        return GetGlobalResourceObject(classKey, resourceKey, null /*objType*/, null /*propName*/, null /*culture*/);
    }

    internal static object GetGlobalResourceObject(string classKey,
        string resourceKey, Type objType, string propName, CultureInfo culture)
    {

        IResourceProvider resourceProvider = GetGlobalResourceProvider(classKey);
        return GetResourceObject(resourceProvider, resourceKey, culture,
            objType, propName);
    }

    private static IResourceProvider GetGlobalResourceProvider(string classKey)
    {
        throw new NotImplementedException("GetGlobalResourceProvider");
    }

    internal static object GetResourceObject(IResourceProvider resourceProvider,
        string resourceKey, CultureInfo culture)
    {
        return GetResourceObject(resourceProvider, resourceKey, culture,
            null /*objType*/, null /*propName*/);
    }

    internal static object GetResourceObject(IResourceProvider resourceProvider,
        string resourceKey, CultureInfo culture, Type objType, string propName)
    {

        if (resourceProvider == null)
            return null;

        object o = resourceProvider.GetObject(resourceKey, culture);

        // If no objType/propName was provided, return the object as is
        if (objType == null)
            return o;

        // Also, if the object from the resource is not a string, return it as is
        string s = o as String;
        if (s == null)
            return o;

        // If they were provided, perform the appropriate conversion
        return ObjectFromString(s, objType, propName);
    }

    private static object ObjectFromString(string value, Type objType, string propName)
    {

        // Get the PropertyDescriptor for the property
        PropertyDescriptor pd = TypeDescriptor.GetProperties(objType)[propName];
        Debug.Assert(pd != null);
        if (pd == null) return null;

        // Get its type descriptor
        TypeConverter converter = pd.Converter;
        Debug.Assert(converter != null);
        if (converter == null) return null;

        // Perform the conversion
        return converter.ConvertFromInvariantString(value);
    }
}

internal class BaseResourcesBuildProvider : BuildProvider
{
    public static string DefaultResourcesNamespace { get; internal set; }

    public void DontGenerateStronglyTypedClass()
    {
        throw new NotImplementedException();
    }
}

internal static class HttpRuntime2
{


    internal static string GetSafePath(string path) {
        if (String.IsNullOrEmpty(path))
            return path;

        try {
            // if (HasPathDiscoveryPermission(path)) // could throw on bad filenames
            return path;
        }
        catch {
        }

        return Path.GetFileName(path);
    }

    internal static object CreateNonPublicInstance(Type type)
    {
        // Check if the type is null
        if (type == null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        // Check if the type is not public
        if (!type.IsPublic)
        {
            // Use reflection to create an instance of the non-public class
            return Activator.CreateInstance(type, true);
        }

        throw new ArgumentException("The type must be non-public.", nameof(type));
    }

    internal static object FastCreatePublicInstance(Type postProcessorType)
    {
        // Check if the type is null
        if (postProcessorType == null)
        {
            throw new ArgumentNullException(nameof(postProcessorType));
        }

        // Check if the type is public
        if (postProcessorType.IsPublic)
        {
            // Use Activator to create an instance of the public class
            return Activator.CreateInstance(postProcessorType);
        }

        throw new ArgumentException("The type must be public.", nameof(postProcessorType));
    }



    internal const string codegenDirName = "Temporary ASP.NET Files";
    internal const string profileFileName = "profileoptimization.prof";

    internal static byte[] s_autogenKeys = new byte[1024];

    //
    // Names of special ASP.NET directories
    //

    internal const string BinDirectoryName = "bin";
    internal const string CodeDirectoryName = "App_Code";
    internal const string WebRefDirectoryName = "App_WebReferences";
    internal const string ResourcesDirectoryName = "App_GlobalResources";
    internal const string LocalResourcesDirectoryName = "App_LocalResources";
    internal const string DataDirectoryName = "App_Data";
    internal const string ThemesDirectoryName = "App_Themes";
    internal const string GlobalThemesDirectoryName = "Themes";
    internal const string BrowsersDirectoryName = "App_Browsers";

    private static string DirectorySeparatorString = new string(Path.DirectorySeparatorChar, 1);
    private static string DoubleDirectorySeparatorString = new string(Path.DirectorySeparatorChar, 2);
    private static char[] s_InvalidPhysicalPathChars = { '/', '?', '*', '<', '>', '|', '"' };
    internal static NamedPermissionSet NamedPermissionSet { get; set;}


    //
    // App domain related
    //

    private static String _tempDir;
    private static String _codegenDir;
    private static String _appDomainAppId;
    private static String _appDomainAppPath;
    private static VirtualPath _appDomainAppVPath;
    private static String _appDomainId;

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public static String CodegenDir {
        get {
            String path = CodegenDirInternal;
            // Todo : Migration
            //InternalSecurityPermissions.PathDiscovery(path).Demand();
            return path;
        }
    }

    internal static string CodegenDirInternal {
        get
        {
            if (_codegenDir == null)
            {
                string dynamicAssemblyPath = AppDomain.CurrentDomain.DynamicDirectory;
                if (dynamicAssemblyPath == null)
                {
                    // TODO: Migration
                    // DynamicDirectory is null, use a fallback directory
                    dynamicAssemblyPath = Path.Combine(Path.GetTempPath(), "MyDynamicAssemblies");
                    // Ensure the directory exists
                    Directory.CreateDirectory(dynamicAssemblyPath);
                }

                _codegenDir = dynamicAssemblyPath;
            }
            return _codegenDir;
        }
    }

    internal static string TempDirInternal {
        get { return _tempDir; }
    }


    internal static bool HasAspNetHostingPermission(AspNetHostingPermissionLevel level) {

        // Make sure we have already initialized the trust level
        //



        // If we don't have a NamedPermissionSet, we're in full trust
        if (NamedPermissionSet == null)
            return true;

        AspNetHostingPermission permission = (AspNetHostingPermission)NamedPermissionSet.GetPermission(
            typeof(AspNetHostingPermission));
        if (permission == null)
            return false;

        return (permission.Level >= level);
    }


    internal static VirtualPath CodeDirectoryVirtualPath {
        get { return _appDomainAppVPath.Combine(CodeDirectoryName); }
    }

    internal static VirtualPath ResourcesDirectoryVirtualPath {
        get { return _appDomainAppVPath.Combine(ResourcesDirectoryName); }
    }

    internal static VirtualPath WebRefDirectoryVirtualPath {
        get { return _appDomainAppVPath.Combine(WebRefDirectoryName); }
    }

    internal static string BinDirectoryInternal {
        get { return Path.Combine(HttpRuntime.AppDomainAppPath, HttpRuntime2.BinDirectoryName) + Path.DirectorySeparatorChar; }

    }

    internal static string AppDomainAppPathInternal {
        get { return HttpRuntime.AppDomainAppPath; }
    }

    public static object CreatePublicInstanceByWebObjectActivator(Type buildProviderType)
    {
        return Activator.CreateInstance(buildProviderType);
    }
}

internal class BuildResultCompiledAssembly : BuildResult
{
    public Assembly ResultAssembly { get; internal set; }
}

internal static class FastPropertyAccessor
{
    internal static object GetProperty(object obj, string name, bool inDesigner)
    {
        // TODO: Make "fast"
        return obj.GetType().GetProperty(name).GetValue(obj);
    }

    internal static void SetProperty(object obj, string name, object objectValue, bool inDesigner)
    {
        // TODO: Make "fast"
        obj.GetType().GetProperty(name).SetValue(obj, objectValue);
    }
}
