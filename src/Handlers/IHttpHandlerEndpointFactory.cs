// MIT License.

using System.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing.Patterns;

namespace Microsoft.AspNetCore.SystemWebAdapters.HttpHandlers;

internal interface IHttpHandlerEndpointFactory
{
    Endpoint Create(IHttpHandler handler);

    EndpointBuilder CreateBuilder(RoutePattern pattern);
}
