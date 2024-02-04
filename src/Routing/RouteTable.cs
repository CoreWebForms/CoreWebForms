// MIT License.

using Microsoft.Extensions.DependencyInjection;
using WebForms;

namespace System.Web.Routing;

public class RouteTable
{
    private RouteTable()
    {
    }

    public static RouteCollection Routes => HttpRuntimeHelper.Services.GetRequiredService<RouteCollection>();
}
