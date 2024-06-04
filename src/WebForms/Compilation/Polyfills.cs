// MIT License.

using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Web.Compilation;

namespace System.Web.UI;

internal class BuildManagerHost
{
    public static bool InClientBuildManager { get; internal set; }
}
internal class BuildResult
{
    public IEnumerable VirtualPathDependencies { get; internal set; }
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

internal class BaseResourcesBuildProvider
{
    public static string DefaultResourcesNamespace { get; internal set; }
}

internal static class BuildManager
{
    public static bool PrecompilingForDeployment { get; internal set; }
    public static string UpdatableInheritReplacementToken { get; internal set; }
    public static Assembly AppResourcesAssembly { get; internal set; }
    public static bool PrecompilingForUpdatableDeployment { get; internal set; }

    internal static BuildResult GetBuildResultFromCache(string cacheKey)
    {
        throw new NotImplementedException("GetBuildResultFromCache");
    }

    internal static string GetLocalResourcesAssemblyName(VirtualPath virtualDir)
    {
        throw new NotImplementedException("GetLocalResourcesAssemblyName");
    }

    internal static TextWriter GetUpdatableDeploymentTargetWriter(VirtualPath currentVirtualPath, Encoding fileEncoding)
        => new StringWriter();

    internal static object GetVPathBuildResult(VirtualPath virtualPath)
    {
        throw new NotImplementedException("GetVPathBuildResult");
    }

    internal static void ReportParseError(ParserError parseError)
    {
        throw new NotImplementedException("ReportParseError");
    }

    internal static void ThrowIfPreAppStartNotRunning()
    {
        throw new NotImplementedException("ThrowIfPreAppStartNotRunning");
    }

    internal static void ValidateCodeFileVirtualPath(VirtualPath codeFileVirtualPath)
    {
    }
}

internal static class HttpRuntime2
{
    internal static RootBuilder CreateNonPublicInstance(Type fileLevelBuilderType)
    {
        throw new NotImplementedException();
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
