// MIT License.

using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.SystemWebAdapters.HttpHandlers;

public interface IHttpHandlerManager
{
    IChangeToken GetChangeToken();

    IEnumerable<IHttpHandlerMetadata> GetHandlerMetadata();
}
