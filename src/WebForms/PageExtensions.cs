// MIT License.

using System.Web.UI;
using Microsoft.Extensions.FileProviders;

namespace Microsoft.AspNetCore.Builder;

public static class PageExtensions
{
    public static void UseWebFormsScripts(this IApplicationBuilder app)
    {
        var provider = new EmbeddedFileProvider(typeof(Page).Assembly, "System.Web.UI.WebControls.RuntimeScripts");
        var files = new StaticFileOptions
        {
            RequestPath = "/__webforms/scripts/system.web",
            FileProvider = provider,
        };

        app.UseStaticFiles(files);
    }
}
