// MIT License.

using System.Collections;
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
        throw new NotImplementedException();
    }

    internal static object ParseExpression(string fullResourceKey)
    {
        throw new NotImplementedException();
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
        throw new NotImplementedException();
    }

    internal static string GetLocalResourcesAssemblyName(VirtualPath virtualDir)
    {
        throw new NotImplementedException();
    }

    internal static TextWriter GetUpdatableDeploymentTargetWriter(VirtualPath currentVirtualPath, Encoding fileEncoding)
        => new StringWriter();
    
    internal static object GetVPathBuildResult(VirtualPath virtualPath)
    {
        throw new NotImplementedException();
    }

    internal static void ReportParseError(ParserError parseError)
    {
        throw new NotImplementedException();
    }

    internal static void ThrowIfPreAppStartNotRunning()
    {
        throw new NotImplementedException();
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

public class FastPropertyAccessor
{
    internal static object GetProperty(object obj, string name, bool inDesigner)
    {
        throw new NotImplementedException();
    }

    internal static void SetProperty(object obj, string name, object objectValue, bool inDesigner)
    {
        throw new NotImplementedException();
    }
}
