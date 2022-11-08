// MIT License.

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
        _mapped.Remove(path);

        while (GetItem(path) is { } existing)
        {
            _items.Remove(existing);
        }
    }

    public IDisposable PauseUpdates() => Token.Pause();

    private RouteItem? GetItem(string path) => _items.FirstOrDefault(i => i.Path == path);

    public void Add(string path, Type type)
    {
        _items.Add(RouteItem.Create(path, type));
        Reset();
    }

    public void Add<T>(string path)
        where T : IHttpHandler
    {
        _items.Add(RouteItem.Create<T>(path));
        Reset();
    }

    private void Reset() => Token.Reset();

    public void Add(string path, IHttpHandler handler)
    {
        _items.Add(RouteItem.Create(handler, path));
        Reset();
    }

    public void MapPageRoute(string routeName, string routeUrl, string path)
    {
        AddMapping(path, new(routeName, RoutePatternFactory.Parse(routeUrl)));
        Reset();
    }

    private void AddMapping(string path, MappedRoute route)
    {
        if (_mapped.TryGetValue(path, out var result))
        {
            result.Add(route);
        }
        else
        {
            _mapped.Add(path, new() { route });
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

    internal sealed record MappedRoute(string Name, RoutePattern Pattern);
}
