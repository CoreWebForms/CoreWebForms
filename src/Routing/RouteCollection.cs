// MIT License.

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Extensions.Options;

namespace System.Web.Routing;

public sealed class RouteCollection : IDisposable
{
    private readonly Dictionary<PathString, MappedRoute> _mapped = [];
    private readonly IOptions<RouteOptions> _options;
    private readonly TemplateBinderFactory _templateBinder;
    private readonly ReaderWriterLockSlim _rwLock = new();

    internal RouteCollection(TemplateBinderFactory template, IOptions<RouteOptions> options)
    {
        _options = options;
        _templateBinder = template;
    }

    public void MapPageRoute(string routeName, string routeUrl, string path)
    {
        _rwLock.EnterWriteLock();

        try
        {
            var normalized = path.Trim('~');
            var pattern = RoutePatternFactory.Parse(routeUrl);
            var route = new MappedRoute(routeName, pattern, new TemplateMatcher(new RouteTemplate(pattern), []));

            _mapped[normalized] = route;
        }
        finally
        {
            _rwLock.ExitWriteLock();
        }
    }

    internal bool TryGetMapped(PathString path, out PathString newPath, [MaybeNullWhen(false)] out Microsoft.AspNetCore.Routing.RouteValueDictionary result)
    {
        result = [];

        _rwLock.EnterReadLock();

        try
        {
            foreach (var (key, m) in _mapped)
            {
                if (m.Matcher.TryMatch(path, result))
                {
                    newPath = key;
                    return true;
                }

                result.Clear();
            }

            newPath = default;
            return false;
        }
        finally
        {
            _rwLock.ExitReadLock();
        }
    }

    public VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values)
    {
        throw new NotImplementedException("Not implemented yet");
    }

    public VirtualPathData GetVirtualPath(RequestContext requestContext, string name, RouteValueDictionary values)
    {
        var httpContext = requestContext.HttpContext;

        if (!string.IsNullOrEmpty(name) && httpContext != null)
        {
            if (GetMappedRoute(name, out var routePattern))
            {
                var path = _templateBinder.Create(routePattern).BindValues(values);

                return new VirtualPathData
                {
                    VirtualPath = NormalizeVirtualPath(path) ?? string.Empty
                };
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

    //The Algo can be improved , let's discuss.
    private bool GetMappedRoute(string routeName, [MaybeNullWhen(false)] out RoutePattern routePattern)
    {
        _rwLock.EnterReadLock();

        try
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
        finally
        {
            _rwLock.ExitReadLock();
        }
    }

    [return: NotNullIfNotNull(nameof(url))]
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

    public void Dispose()
    {
        _rwLock.Dispose();
    }

    private sealed record MappedRoute(string Name, RoutePattern Pattern, TemplateMatcher Matcher);
}
