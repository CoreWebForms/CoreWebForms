// MIT License.

using Microsoft.AspNetCore.Builder;

namespace System.Web.Routing;

public static class HandlerEndpointBuilder
{
    public static EndpointBuilder Create(string path, Type type)
        => RouteItem.Create(path, type).GetBuilder();

    public static EndpointBuilder Create(string path, IHttpHandler handler)
        => RouteItem.Create(handler, path).GetBuilder();
}
