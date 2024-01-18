// MIT License.

using System.Globalization;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using VirtualPathData = System.Web.Routing.VirtualPathData;

namespace System.Web.Routing;

public class RouteCollection
{
    private readonly List<RouteItem> _items = new();
    private readonly Dictionary<string, HashSet<MappedRoute>> _mapped = new();
    private CancellationChangeTokenSource? _changeToken;
    private RouteOptions? _options;

    internal RouteCollection()
    {
    }

    public void Remove(string path)
    {
        while (GetItem(path) is { } existing)
        {
            _items.Remove(existing);
        }
    }

    public IDisposable GetWriteLock() => Token.GetWriteLock();

    public IDisposable GetReadLock() => Token.GetReadLock();

    private RouteItem? GetItem(string path) => _items.FirstOrDefault(i => i.Path == path);

    public void Add(string path, Type type)
    {
        _items.Add(RouteItem.Create(path, type));
        _changeToken?.OnChange();
    }

    public void Add<T>(string path)
        where T : IHttpHandler
    {
        _items.Add(RouteItem.Create<T>(path));
        _changeToken?.OnChange();
    }

    public void Add(string path, IHttpHandler handler)
    {
        _items.Add(RouteItem.Create(handler, path));
        _changeToken?.OnChange();
    }

    public void MapPageRoute(string routeName, string routeUrl, string path)
    {
        AddMapping(path, new(routeName, RoutePatternFactory.Parse(routeUrl)));
        _changeToken?.OnChange();
    }

    private void AddMapping(string path, MappedRoute route)
    {
        var normalized = path.Trim('~');

        if (_mapped.TryGetValue(normalized, out var result))
        {
            result.Add(route);
        }
        else
        {
            _mapped.Add(normalized, new() { route });
        }
    }

    private CancellationChangeTokenSource Token => _changeToken ??= new();

    internal IChangeToken GetChangeToken() => Token.GetChangeToken();

    internal IEnumerable<RouteItem> GetRoutes()
    {
        foreach (var item in _items)
        {
            if (item.Path is { } path && _mapped.TryGetValue(path, out var result))
            {
                foreach (var mapped in result)
                {
                    yield return item with
                    {
                        Pattern = mapped.Pattern,
                        Path = mapped.Name
                    };
                }
            }
            else
            {
                yield return item;
            }
        }
    }

    public void Replace(PathString path, IHttpHandler handler)
    {
        Remove(path);
        Add(path, handler);
    }

    public void Replace(PathString path, Type type)
    {
        Remove(path);
        Add(path, type);
    }

    public VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values)
    {
        throw new NotImplementedException("Not implemented yet");
    }

    public VirtualPathData GetVirtualPath(RequestContext requestContext, string name, RouteValueDictionary values)
    {
        var httpContext = requestContext.HttpContext; //This HttpContextWrapper

        if (!string.IsNullOrEmpty(name) && httpContext != null)
        {
            EnsureOptions(httpContext);
            RoutePattern? routePattern;
            bool routeFound;
            var templateBinderFactory = httpContext.GetService<TemplateBinderFactory>();
            using (GetReadLock())
            {
                routeFound = GetMappedRoute(name, out routePattern);
            }
            if (routeFound && templateBinderFactory != null && routePattern != null)
            {
                var path = templateBinderFactory.Create(routePattern).BindValues(values);
                path = NormalizeVirtualPath(path);
                var virTualPathData = new VirtualPathData();
                virTualPathData.VirtualPath = path ?? string.Empty;
                return virTualPathData;
            }
            else
            {
                throw new ArgumentException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "A route named '{0}' could not be found in the route collection.",
                        name),
                    nameof(name));
            }
        }
        else
        {
            return GetVirtualPath(requestContext, values);
        }
    }

    internal sealed record MappedRoute(string Name, RoutePattern Pattern);

    //The Algo can be improved , let's discuss.
    private bool GetMappedRoute(string routeName, out RoutePattern? routePattern)
    {
        routePattern = null;
        foreach (var mappedRoutes in _mapped.Values)
        {
            foreach (MappedRoute mapRoute in mappedRoutes)
            {
                if (string.CompareOrdinal(mapRoute.Name, routeName) == 0)
                {
                    routePattern = mapRoute.Pattern;
                    return true;
                }
            }
        }
        return false;
    }

    private string? NormalizeVirtualPath(string? url)
    {
        if (url == null)
        {
            return url;
        }

        if (!string.IsNullOrEmpty(url) && (_options != null && (_options.LowercaseUrls || _options.AppendTrailingSlash)))
        {
            var indexOfSeparator = url.AsSpan().IndexOfAny('?', '#');
            var urlWithoutQueryString = url;
            var queryString = string.Empty;

            if (indexOfSeparator != -1)
            {
                urlWithoutQueryString = url.Substring(0, indexOfSeparator);
                queryString = url.Substring(indexOfSeparator);
            }

            if (_options.LowercaseUrls)
            {
                urlWithoutQueryString = urlWithoutQueryString.ToLowerInvariant();
            }

            if (_options.LowercaseUrls && _options.LowercaseQueryStrings)
            {
                queryString = queryString.ToLowerInvariant();
            }

            if (_options.AppendTrailingSlash && !urlWithoutQueryString.EndsWith('/'))
            {
                urlWithoutQueryString += "/";
            }

            // queryString will contain the delimiter ? or # as the first character, so it's safe to append.
            url = urlWithoutQueryString + queryString;

            return url;
        }

        return url;
    }

    private void EnsureOptions(HttpContextBase httpContext)
    {
        _options ??= httpContext.GetService<IOptions<RouteOptions>>()?.Value;
    }

}
