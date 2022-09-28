// MIT License.

using System.Reflection;
using System.Web.UI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNetCore.Builder;

public static class PageEndpointBuilder
{
    public static void MapAspxPages(this IEndpointRouteBuilder builder, params Assembly[] assemblies)
    {
        if (assemblies.Length == 0)
        {
            if (Assembly.GetEntryAssembly() is { } entry)
            {
                assemblies = new[] { entry };
            }
            else
            {
                return;
            }
        }

        var dataSource = builder.GetPageDataSource();

        foreach (var assembly in assemblies)
        {
            foreach (var type in assembly.GetExportedTypes())
            {
                if (type.IsAssignableTo(typeof(Page)))
                {
                    dataSource.Add(type, false);
                }
            }
        }
    }

    public static void MapAspxPage(this IEndpointRouteBuilder endpoints, Type page, PathString path)
        => endpoints.GetPageDataSource().Add(page, path);

    public static void MapAspxPage<TPage>(this IEndpointRouteBuilder endpoints, PathString path)
        where TPage : Page
        => endpoints.GetPageDataSource().Add(typeof(TPage), path);

    private static PageEndpointDataSource GetPageDataSource(this IEndpointRouteBuilder endpoints)
    {
        var dataSource = endpoints.DataSources.OfType<PageEndpointDataSource>().FirstOrDefault();

        if (dataSource is null)
        {
            dataSource = new PageEndpointDataSource();
            endpoints.DataSources.Add(dataSource);
        }

        return dataSource;
    }
}
