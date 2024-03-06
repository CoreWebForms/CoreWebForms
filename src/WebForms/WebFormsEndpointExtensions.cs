// MIT License.

using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNetCore.Builder;

public static class WebFormsEndpointExtensions
{
    public static void MapWebForms(this IEndpointRouteBuilder endpoints)
    {
        // This ensures they're mapped which always returns the same convention builder
        endpoints.MapHttpHandlers();
       }
}
