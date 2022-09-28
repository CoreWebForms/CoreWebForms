// MIT License.

using System.Reflection;

#nullable disable

namespace System.Web.UI;
internal interface IScriptResourceMapping
{
    IScriptResourceDefinition GetDefinition(string resourceName);
    IScriptResourceDefinition GetDefinition(string resourceName, Assembly resourceAssembly);
}
