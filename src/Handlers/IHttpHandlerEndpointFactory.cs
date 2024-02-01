// MIT License.

using System.Web;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.SystemWebAdapters.HttpHandlers;

internal interface IHttpHandlerEndpointFactory
{
    Endpoint Create(IHttpHandler handler);
}
