//MIT License

using System.Diagnostics.CodeAnalysis;

namespace System.Web.Routing;

public class RequestContext
{
    public RequestContext()
    {
    }

    [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors",
        Justification = "The virtual property setters are only to support mocking frameworks, in which case this constructor shouldn't be called anyway.")]
    public RequestContext(HttpContextBase httpContext, RouteData routeData)
    {
        if (httpContext == null)
        {
            throw new ArgumentNullException(nameof(httpContext));
        }
        if (routeData == null)
        {
            throw new ArgumentNullException(nameof(routeData));
        }
        HttpContext = httpContext;
        RouteData = routeData;
    }

    public virtual HttpContextBase? HttpContext { get; set; }

    public virtual RouteData? RouteData { get; set; }
}
