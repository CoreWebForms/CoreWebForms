// MIT License.

using System.Web;
using System.Web.SessionState;

namespace Microsoft.AspNetCore.SystemWebAdapters.HttpHandlers;

public interface IHttpHandlerMetadata
{
    SessionStateBehavior Behavior { get; }

    string Route { get; }

    IHttpHandler Create(HttpContextCore context);
}

