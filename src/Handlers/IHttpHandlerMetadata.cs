// MIT License.

using System.Web;

namespace Microsoft.AspNetCore.SystemWebAdapters.HttpHandlers;

public interface IHttpHandlerMetadata
{
    ValueTask<IHttpHandler> Create(HttpContextCore context);
}

