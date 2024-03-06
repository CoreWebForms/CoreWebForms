// MIT License.

using System.Globalization;
using System.Reflection;

#nullable disable

namespace System.Web.UI;

internal interface IScriptResourceHandler
{
    string GetScriptResourceUrl(Assembly assembly, string resourceName, CultureInfo culture, bool zip);

    string GetWebResourceUrl(Type type, string resourceName, bool htmlEncoded, IScriptManager scriptManager, bool enableCdn);
}
