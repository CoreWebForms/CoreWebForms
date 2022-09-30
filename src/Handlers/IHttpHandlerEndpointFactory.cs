// MIT License.

using Microsoft.AspNetCore.Http;

namespace System.Web;

internal interface IHttpHandlerEndpointFactory
{
    Endpoint Create(IHttpHandler handler);
}
