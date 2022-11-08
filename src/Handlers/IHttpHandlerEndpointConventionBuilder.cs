// MIT License.

namespace Microsoft.AspNetCore.Builder;

public interface IHttpHandlerEndpointConventionBuilder : IEndpointConventionBuilder
{
    System.Web.Routing.RouteCollection Routes { get; }
}
