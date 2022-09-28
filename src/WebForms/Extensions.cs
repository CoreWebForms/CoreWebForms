// MIT License.

using System.Web.Caching;
using System.Web.UI;

namespace System.Web;

internal static class Extensions
{
    public static void Dispose(this CacheDependency dep)
    {
    }

    public static bool IsPost(this HttpRequest request) => string.Equals("POST", request.HttpMethod, StringComparison.OrdinalIgnoreCase);

    public static void InvokeCancellableCallback(this HttpContext context, WaitCallback callback, object state)
        => callback(state);

    public static string MapPath(this HttpRequest request, string path, VirtualPath vpath, bool crossApp)
    {
        return vpath.Path;
    }

    public static TemplateControl TemplateControl(this HttpContext context)
        => ((HttpContextCore)context).Features.Get<TemplateControl>();

    public static void TemplateControl(this HttpContext context, TemplateControl control)
        => ((HttpContextCore)context).Features.Set<TemplateControl>(control);

    public static VirtualPath CurrentExecutionFilePathObject(this HttpRequest request)
    {
        return request.Path;
    }
}
