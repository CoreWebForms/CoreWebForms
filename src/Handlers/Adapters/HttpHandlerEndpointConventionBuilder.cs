// MIT License.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal sealed class HttpHandlerEndpointConventionBuilder : EndpointDataSource, IHttpHandlerEndpointConventionBuilder
{
    private List<Action<EndpointBuilder>> _conventions = new();

    internal HttpHandlerEndpointConventionBuilder(System.Web.Routing.RouteCollection routes)
    {
        Routes = routes;
    }

    public System.Web.Routing.RouteCollection Routes { get; }

    public override IReadOnlyList<Endpoint> Endpoints
    {
        get
        {
            var endpoints = new List<Endpoint>();

            foreach (var route in Routes.GetRoutes())
            {
                var builder = route.GetBuilder();

                foreach (var convention in _conventions)
                {
                    convention(builder);
                }

                endpoints.Add(builder.Build());
            }

            return endpoints;
        }
    }

    public void Add(Action<EndpointBuilder> convention)
        => (_conventions ??= new()).Add(convention);

    public override IChangeToken GetChangeToken() => Routes.GetChangeToken();
}
