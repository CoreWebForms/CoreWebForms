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
internal class PageParserFilter
{
    public bool AllowCode { get; internal set; }

    internal static PageParserFilter Create(PagesSection pagesConfig, VirtualPath currentVirtualPath, TemplateParser templateParser)
    {
        throw new NotImplementedException();
    }

    internal bool AllowBaseType(Type baseType)
    {
        throw new NotImplementedException();
    }

    internal bool AllowControlInternal(Type childType, ControlBuilder subBuilder)
    {
        throw new NotImplementedException();
    }

    internal bool AllowServerSideInclude(string virtualPathString)
    {
        throw new NotImplementedException();
    }

    internal bool AllowVirtualReference(CompilationSection compConfig, VirtualPath virtualPath)
    {
        throw new NotImplementedException();
    }

    internal CompilationMode GetCompilationMode(CompilationMode compilationMode)
    {
        throw new NotImplementedException();
    }

    internal Type GetNoCompileUserControlType()
    {
        throw new NotImplementedException();
    }

    internal void OnDependencyAdded()
    {
        throw new NotImplementedException();
    }

    internal void OnDirectDependencyAdded()
    {
        throw new NotImplementedException();
    }

    internal void ParseComplete(RootBuilder rootBuilder)
    {
        throw new NotImplementedException();
    }

    internal void PreprocessDirective(string directiveName, IDictionary directive)
    {
        throw new NotImplementedException();
    }

    internal bool ProcessCodeConstruct(CodeConstructType codeConstructType, string code)
    {
        throw new NotImplementedException();
    }

    internal bool ProcessDataBindingAttribute(string controlId, string attributeName, string code)
    {
        throw new NotImplementedException();
    }

    internal bool ProcessEventHookup(string controlId, string eventName, string handlerName)
    {
        throw new NotImplementedException();
    }
}

internal class BaseCodeDomTreeGenerator
{
    internal static string defaultNamespace;

    internal static bool IsAspNetNamespace(string @namespace)
    {
        throw new NotImplementedException();
    }
}

internal class ControlBuilderInterceptor
{
    internal void PreControlBuilderInit(ControlBuilder controlBuilder, TemplateParser parser, ControlBuilder parentBuilder, Type type, string tagName, string id, IDictionary attribs, IDictionary additionalState)
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
    {
        throw new NotImplementedException();
    }

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
        throw new NotImplementedException();
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

public enum CodeConstructType
{
    CodeSnippet,            // <% ... %>
    ExpressionSnippet,      // <%= ... %>
    DataBindingSnippet,     // <%# ... %>
    ScriptTag,              // <script runat="server">...</script>
    EncodedExpressionSnippet // <%: ... %>
}


internal class BaseTemplateCodeDomTreeGenerator
{
    internal static string skinIDPropertyName;
}

