// MIT License.

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace System.Web.Routing;

public sealed class RouteCollection
{
    private readonly Dictionary<string, MappedRoute> _mapped = new();
    private readonly IOptions<RouteOptions> _options;
    private readonly TemplateBinderFactory _templateBinder;

    internal RouteCollection(TemplateBinderFactory template, IOptions<RouteOptions> options)
    {
        _options = options;
        _templateBinder = template;
    }

    public void MapPageRoute(string routeName, string routeUrl, string path)
    {
        var pattern = RoutePatternFactory.Parse(routeUrl);
        AddMapping(path, new(routeName, pattern, _templateBinder.Create(pattern)));
    }

    internal bool TryGetMapped(PathString path, out PathString newPath, [MaybeNullWhen(false)] out Microsoft.AspNetCore.Routing.RouteValueDictionary result)
    {
        result = [];

        foreach (var (key, m) in _mapped)
        {
            var re = new RouteTemplate(m.Pattern);
            var matcher = new TemplateMatcher(re, result);

            if (matcher.TryMatch(path, result))
            {
                newPath = key;
                return true;
            }

            result.Clear();
        }

        newPath = default;
        return false;
    }

    private void AddMapping(string path, MappedRoute route)
    {
        var normalized = path.Trim('~');

        _mapped[normalized] = route;
    }

    public VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values)
    {
        throw new NotImplementedException("Not implemented yet");
    }

    // TODO implement better
    private static IDisposable GetReadLock() => EmptyDisposable.Singleton;

    public VirtualPathData GetVirtualPath(RequestContext requestContext, string name, RouteValueDictionary values)
    {
        var httpContext = requestContext.HttpContext; //This HttpContextWrapper

        if (!string.IsNullOrEmpty(name) && httpContext != null)
        {
            RoutePattern? routePattern;
            bool routeFound;

            using (GetReadLock())
            {
                routeFound = GetMappedRoute(name, out routePattern);
            }

            if (routeFound && _templateBinder != null && routePattern != null)
            {
                var path = _templateBinder.Create(routePattern).BindValues(values);
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

    internal sealed record MappedRoute(string Name, RoutePattern Pattern, TemplateBinder Binder);

    //The Algo can be improved , let's discuss.
    private bool GetMappedRoute(string routeName, out RoutePattern? routePattern)
    {
        routePattern = null;
        foreach (var mapRoute in _mapped.Values)
        {
            if (string.Equals(mapRoute.Name, routeName, StringComparison.OrdinalIgnoreCase))
            {
                routePattern = mapRoute.Pattern;
                return true;
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

        var options = _options.Value;

        if (!string.IsNullOrEmpty(url) && (options != null && (options.LowercaseUrls || options.AppendTrailingSlash)))
        {
            var indexOfSeparator = url.AsSpan().IndexOfAny('?', '#');
            var urlWithoutQueryString = url;
            var queryString = string.Empty;

            if (indexOfSeparator != -1)
            {
                urlWithoutQueryString = url.Substring(0, indexOfSeparator);
                queryString = url.Substring(indexOfSeparator);
            }

            if (options.LowercaseUrls)
            {
                urlWithoutQueryString = urlWithoutQueryString.ToLowerInvariant();
            }

            if (options.LowercaseUrls && options.LowercaseQueryStrings)
            {
                queryString = queryString.ToLowerInvariant();
            }

            if (options.AppendTrailingSlash && !urlWithoutQueryString.EndsWith('/'))
            {
                urlWithoutQueryString += "/";
            }

            // queryString will contain the delimiter ? or # as the first character, so it's safe to append.
            url = urlWithoutQueryString + queryString;

            return url;
        }

        return url;
    }

    private sealed class EmptyDisposable : IDisposable
    {
        public static EmptyDisposable Singleton { get; } = new();

        public void Dispose()
        {
        }
    }
}
