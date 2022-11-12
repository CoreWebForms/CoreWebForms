// MIT License.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.Primitives;

namespace System.Web.Routing;

public class RouteCollection
{
    private readonly List<RouteItem> _items = new();
    private readonly Dictionary<string, HashSet<MappedRoute>> _mapped = new();
    private CancellationChangeTokenSource? _changeToken;

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

    internal IChangeToken GetChangeToken() => Token;

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

    internal sealed record MappedRoute(string Name, RoutePattern Pattern);
}
