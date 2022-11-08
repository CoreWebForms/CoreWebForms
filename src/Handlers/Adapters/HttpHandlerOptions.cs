// MIT License.

namespace Microsoft.Extensions.DependencyInjection;

public class HttpHandlerOptions
{
    public System.Web.Routing.RouteCollection Routes { get; } = new();
}
