// MIT License.

using System.Web;
using Microsoft.AspNetCore.SystemWebAdapters;
using WebForms.Features;
using HttpContext = System.Web.HttpContext;

namespace WebFormsSample.Dynamic;

public class SampleHttpHandler : IHttpHandler
{
    public void ProcessRequest(HttpContext context)
    {
        var feature = context.AsAspNetCore().Features.Get<IWebFormsCompilationFeature>();
        var availability = feature == null ? "Not available" : "Available";
        context.Response.Write($"IWebFormsCompilationFeature is {availability}");
    }

    public bool IsReusable { get; }
}
