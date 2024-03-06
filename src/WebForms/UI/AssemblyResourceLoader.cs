// MIT License.

using System.Reflection;

namespace System.Web.UI;

internal class AssemblyResourceLoader
{
    internal static string FormatCdnUrl(Assembly assembly, string cdnPath)
    {
        throw new NotImplementedException();
    }

    internal static Assembly GetAssemblyFromType(Type type) => type.Assembly;

    internal static string GetWebResourceUrl(Type type, string resourceName, bool htmlEncoded, IScriptManager scriptManager, bool enableCdn)
    {
        return $"/__webforms/scripts/{resourceName}";
    }

    internal static string GetWebResourceUrl(Type type, string path)
    {
        return $"/__webforms/scripts/{path}";
    }
}
