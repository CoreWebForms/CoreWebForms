// MIT License.

using System.Web.Script;
using System.Web.UI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WebForms.Extensions;

[assembly: TagPrefix("System.Web.UI", "asp")]
[assembly: TagPrefix("System.Web.UI.WebControls", "asp")]
[assembly: AjaxFrameworkAssembly]

namespace Microsoft.Extensions.DependencyInjection;

public static class ScriptManagerExtensions
{
    public static IWebFormsBuilder AddScriptManager(this IWebFormsBuilder builder)
    {
        builder.Services.TryAddSingleton<ScriptResourceHandler>();
        builder.Services.AddSingleton<IScriptResourceHandler>(sp => sp.GetRequiredService<ScriptResourceHandler>());

        return builder;
    }

    public static void MapScriptManager(this IEndpointRouteBuilder endpoints)
    {
        endpoints.Map($"{endpoints.ServiceProvider.GetRequiredService<ScriptResourceHandler>().Prefix}", Results<FileStreamHttpResult, NotFound> (HttpRequest request, [FromServices] ScriptResourceHandler handler) =>
        {
            if (request.Query["s"] is [{ } file] && handler.Resolve(file) is { } resource)
            {
                return TypedResults.Stream(resource);
            }
            else
            {
                return TypedResults.NotFound();
            }
        });
    }
}
