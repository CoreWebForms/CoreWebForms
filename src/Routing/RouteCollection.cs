// MIT License.

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.AspNetCore.SystemWebAdapters.HttpHandlers;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace System.Web.Routing;

public sealed class RouteCollection : IDisposable, IHttpHandlerCollection
{
    private readonly Dictionary<string, (RoutePattern Pattern, NamedHttpHandlerRoute Mapped)> _mapped = [];
    private readonly IOptions<RouteOptions> _options;
    private readonly TemplateBinderFactory _templateBinder;
    private readonly ReaderWriterLockSlim _rwLock = new();
    private readonly CancellationChangeTokenSource _cts = new();

    IEnumerable<NamedHttpHandlerRoute> IHttpHandlerCollection.NamedRoutes
    {
        get
        {
            _rwLock.EnterReadLock();

            try
            {
                return _mapped.Values.Select(m => m.Mapped).ToList();
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
        }
    }


    internal RouteCollection(TemplateBinderFactory template, IOptions<RouteOptions> options)
    {
        _options = options;
        _templateBinder = template;
    }

    public void MapPageRoute(string routeName, string routeUrl, string path)
    {
        _rwLock.EnterWriteLock();

        if (path.StartsWith("~"))
        {
            path = path[1..];
        }

        try
        {
            _mapped.Add(routeName, (RoutePatternFactory.Parse(routeUrl), new(routeName, routeUrl, path)));
        }
        finally
        {
            _rwLock.ExitWriteLock();
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
            var success = _mapped.TryGetValue(routeName, out var result);

            if (success)
            {
                routePattern = result.Pattern;
                return success;
            }

            routePattern = default;
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
        _cts.Dispose();
    }

    IChangeToken IHttpHandlerCollection.GetChangeToken() => _cts.GetChangeToken();

    IEnumerable<IHttpHandlerMetadata> IHttpHandlerCollection.GetHandlerMetadata() => [];
}
