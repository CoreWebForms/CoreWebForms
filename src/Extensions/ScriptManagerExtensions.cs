// MIT License.

using System.Web.UI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.FileProviders;

namespace Microsoft.Extensions.DependencyInjection;

public static class ScriptManagerExtensions
{
    public static IWebFormsBuilder AddScriptManager(this IWebFormsBuilder builder)
    {
        return builder;
    }

    public static void MapScriptManager(this IEndpointRouteBuilder endpoints)
    {
        var provider = new EmbeddedFileProvider(typeof(ScriptManager).Assembly, "System.Web.Script.js.dist");
        var path = "/__webforms/scripts";

        endpoints.MapStaticFiles(provider, path, name => $"AJAX [{name}]");
    }
}
