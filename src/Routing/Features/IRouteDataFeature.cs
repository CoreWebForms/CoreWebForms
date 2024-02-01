// MIT License.

using System.Web.Routing;

namespace Microsoft.AspNetCore.SystemWebAdapters.Features;

internal interface IRouteDataFeature
{
    RequestContext RequestContext { get; }

    RouteData RouteData { get; }
}
