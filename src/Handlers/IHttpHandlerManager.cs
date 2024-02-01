// MIT License.

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.SystemWebAdapters.HttpHandlers;

public interface IHttpHandlerManager
{
    IChangeToken GetChangeToken();

    IEnumerable<EndpointBuilder> GetBuilders();
}
