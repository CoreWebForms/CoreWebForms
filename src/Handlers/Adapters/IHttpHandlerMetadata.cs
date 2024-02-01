// MIT License.

using System.Web;

namespace Microsoft.AspNetCore.SystemWebAdapters;

public interface IHttpHandlerMetadata
{
    ValueTask<IHttpHandler> Create(HttpContextCore context);
}

