// MIT License.

using System.Web;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SystemWebAdapters.HttpHandlers;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Builder;

public static class HttpHandlerBuilderExtensions
{
    public static void MapHttpHandler<T>(this IEndpointRouteBuilder endpoints, string path)
       where T : IHttpHandler
        => endpoints.GetOrCreateBuilder().Add(path, typeof(T));

    public static void MapHttpHandler(this IEndpointRouteBuilder endpoints, string path, Type type)
        => endpoints.GetOrCreateBuilder().Add(path, type);

    public static IEndpointConventionBuilder MapHttpHandlers(this IEndpointRouteBuilder endpoints)
        => endpoints.GetOrCreateBuilder();

    private static HttpHandlerEndpointConventionBuilder GetOrCreateBuilder(this IEndpointRouteBuilder endpoints)
    {
        if (endpoints.DataSources.OfType<HttpHandlerEndpointConventionBuilder>().FirstOrDefault() is not { } existing)
        {
            existing = endpoints.ServiceProvider.GetRequiredService<HttpHandlerEndpointConventionBuilder>();
            endpoints.DataSources.Add(existing);
        }

        return existing;
    }
}
