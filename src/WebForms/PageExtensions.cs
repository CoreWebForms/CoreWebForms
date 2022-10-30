// MIT License.

using System.Web.UI;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace Microsoft.AspNetCore.Builder;

public static class PageExtensions
{
    public static ISystemWebAdapterBuilder AddWebForms(this ISystemWebAdapterBuilder services)
        => services.AddHttpHandlers();

    public static void UseWebFormsScripts(this IApplicationBuilder app)
    {
        var provider = new EmbeddedFileProvider(typeof(Page).Assembly, "System.Web.UI.WebControls.RuntimeScripts");
        var files = new StaticFileOptions
        {
            RequestPath = "/__webforms/scripts",
            FileProvider = provider,
        };

        app.UseStaticFiles(files);
    }
}
