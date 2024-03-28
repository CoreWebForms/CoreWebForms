// MIT License.

using Microsoft.Extensions.DependencyInjection;

namespace System.Web.Routing;

public class RouteTable
{
    private RouteTable()
    {
    }

    public static RouteCollection Routes => HttpRuntime.WebObjectActivator.GetRequiredService<RouteCollection>();
}
