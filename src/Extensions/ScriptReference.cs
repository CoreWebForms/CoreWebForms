// MIT License.

using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;

namespace System.Web.UI;

[
  DefaultProperty("Path"),
  ]
public class ScriptReference : ScriptReferenceBase
{

    [
    Category("Behavior"),
    DefaultValue(""),
    ]
    public string Name { get; set; }

    [
    Category("Behavior"),
    DefaultValue(""),
    ]
    public string Assembly { get; set; }

    internal Assembly GetAssembly()
    {
        return String.IsNullOrEmpty(Assembly) ? null : AssemblyCache.Load(Assembly);
    }

    //TODO https://github.com/twsouthwick/systemweb-adapters-ui/issues/28
    internal Assembly GetAssembly(ScriptManager scriptManager)
    {
        throw new NotImplementedException();
        /*
        // normalizes the effective assembly by redirecting it to the given scriptmanager's
        // ajax framework assembly when it is set to SWE.
        // EffectiveAssembly can't do this since ScriptReference does not have access by itself
        // to the script manager.
        Debug.Assert(scriptManager != null);
        Assembly assembly = EffectiveAssembly;
        if (assembly == null)
        {
            return scriptManager.AjaxFrameworkAssembly;
        }
        else
        {
            return ((assembly == AssemblyCache.SystemWebExtensions) ?
                scriptManager.AjaxFrameworkAssembly :
                assembly);
        }*/
    }
}
