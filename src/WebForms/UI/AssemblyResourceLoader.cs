// MIT License.

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using WebForms;

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
        return HttpRuntimeHelper.Services.GetRequiredService<IScriptResourceHandler>().GetWebResourceUrl(type, resourceName, htmlEncoded, scriptManager, enableCdn);
    }

    internal static string GetWebResourceUrl(Type type, string path)
    {
        return $"/__webforms/scripts/{path}";
    }
}
