// MIT License.

using System.Web.Routing;

namespace Microsoft.AspNetCore.SystemWebAdapters;

public interface IRoutingAdapterFeature
{
    RouteData? RouteData { get; }
}
