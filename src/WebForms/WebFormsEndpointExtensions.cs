// MIT License.

using System.Net;
using System.Web.UI;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.FileProviders;

namespace Microsoft.AspNetCore.Builder;

public static class WebFormsEndpointExtensions
{
    public static void MapWebForms(this IEndpointRouteBuilder endpoints)
    {
        // This ensures they're mapped which always returns the same convention builder
        endpoints.MapHttpHandlers();

        var provider = new EmbeddedFileProvider(typeof(Page).Assembly, "System.Web.UI.WebControls.RuntimeScripts");

        endpoints.MapStaticFiles(provider, "/__webforms/scripts/system.web", path => $"WebForms Static Files [{path}]");
    }
}
