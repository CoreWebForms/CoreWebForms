// MIT License.

using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;
using System.Web.Resources;
using System.Web.Util;

namespace System.Web.UI;
public class ScriptResourceMapping : IScriptResourceMapping
{
    private readonly ConcurrentDictionary<Tuple<string, Assembly>, ScriptResourceDefinition> _definitions =
        new ConcurrentDictionary<Tuple<string, Assembly>, ScriptResourceDefinition>();

    public void AddDefinition(string name, ScriptResourceDefinition definition)
    {
        AddDefinition(name, assembly: AssemblyCache.SystemWebExtensions, definition: definition);
    }

    public void AddDefinition(string name, Assembly assembly, ScriptResourceDefinition definition)
    {
        // dictionary indexer will update the value if it already exists
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException(AtlasWeb.Common_NullOrEmpty, nameof(name));
        }
        if (definition == null)
        {
            throw new ArgumentNullException(nameof(definition));
        }
        if (string.IsNullOrEmpty(definition.ResourceName) && string.IsNullOrEmpty(definition.Path))
        {
            throw new ArgumentException(AtlasWeb.ScriptResourceDefinition_NameAndPathCannotBeEmpty, nameof(definition));
        }
        EnsureAbsoluteOrAppRelative(definition.Path);
        EnsureAbsoluteOrAppRelative(definition.DebugPath);
        EnsureAbsoluteOrAppRelative(definition.CdnPath);
        EnsureAbsoluteOrAppRelative(definition.CdnDebugPath);
        assembly = NormalizeAssembly(assembly);
        _definitions[new Tuple<string, Assembly>(name, assembly)] = definition;
    }

    public void Clear()
    {
        _definitions.Clear();
    }

    private static void EnsureAbsoluteOrAppRelative(string path)
    {
        if (!string.IsNullOrEmpty(path) &&
            !UrlPath.IsAppRelativePath(path) && // ~/foo..
            !UrlPath.IsRooted(path) && // /foo
            !Uri.IsWellFormedUriString(path, UriKind.Absolute))
        { // http://...
            throw new InvalidOperationException(
                string.Format(CultureInfo.InvariantCulture, AtlasWeb.ScriptResourceDefinition_InvalidPath, path));
        }
    }

    public ScriptResourceDefinition GetDefinition(string name)
    {
        return GetDefinition(name, AssemblyCache.SystemWebExtensions);
    }

    public ScriptResourceDefinition GetDefinition(string name, Assembly assembly)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException(AtlasWeb.Common_NullOrEmpty, nameof(name));
        }
        ScriptResourceDefinition definition;
        assembly = NormalizeAssembly(assembly);
        _definitions.TryGetValue(new Tuple<string, Assembly>(name, assembly), out definition);
        return definition;
    }

    public ScriptResourceDefinition GetDefinition(ScriptReference scriptReference)
    {
        if (scriptReference == null)
        {
            throw new ArgumentNullException(nameof(scriptReference));
        }
        string name = scriptReference.Name;
        Assembly assembly = null;
        ScriptResourceDefinition definition = null;
        if (!string.IsNullOrEmpty(name))
        {
            assembly = scriptReference.GetAssembly();
            definition = ScriptManager.ScriptResourceMapping.GetDefinition(name, assembly);
        }
        return definition;
    }

    public ScriptResourceDefinition RemoveDefinition(string name)
    {
        return RemoveDefinition(name, AssemblyCache.SystemWebExtensions);
    }

    public ScriptResourceDefinition RemoveDefinition(string name, Assembly assembly)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException(AtlasWeb.Common_NullOrEmpty, nameof(name));
        }
        ScriptResourceDefinition definition;
        assembly = NormalizeAssembly(assembly);
        _definitions.TryRemove(new Tuple<string, Assembly>(name, assembly), out definition);
        return definition;
    }

    #region IScriptResourceMapping Members
    IScriptResourceDefinition IScriptResourceMapping.GetDefinition(string name)
    {
        return GetDefinition(name);
    }

    IScriptResourceDefinition IScriptResourceMapping.GetDefinition(string name, Assembly assembly)
    {
        return GetDefinition(name, assembly);
    }
    #endregion

    private static Assembly NormalizeAssembly(Assembly assembly)
    {
        if ((assembly != null) && AssemblyCache.IsAjaxFrameworkAssembly(assembly))
        {
            assembly = null;
        }
        return assembly;
    }
}
