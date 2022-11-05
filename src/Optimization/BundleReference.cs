// MIT License.

using System.Web.UI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace System.Web.Optimization;

public class BundleReference : Control
{
    public string? Path { get; set; }

    protected override void Render(HtmlTextWriter writer)
    {
        if (Path is null)
        {
            return;
        }

        var options = ((Microsoft.AspNetCore.Http.HttpContext)Context).RequestServices
            .GetRequiredService<IOptions<BundleReferenceOptions>>()
            .Value;

        if (!options.Bundles.TryGetBundle(Path, out var bundle))
        {
            throw new InvalidOperationException($"Unknown bundle: '{Path}'");
        }

        if (bundle is StyleBundle styles)
        {
            Write(writer, styles);
        }
        else if (bundle is ScriptBundle scripts)
        {
            Write(writer, scripts);
        }
        else
        {
            throw new NotImplementedException($"Unknown ScriptBundle type: {bundle.GetType()}");
        }
    }

    private void Write(HtmlTextWriter writer, ScriptBundle bundle)
    {
        foreach (var include in bundle.Paths)
        {
            writer.Write("<script type=\"text/json\" src=\"");
            writer.Write(include);
            writer.WriteLine("\"></script>");
        }
    }

    private void Write(HtmlTextWriter writer, StyleBundle bundle)
    {
        foreach (var include in bundle.Paths)
        {
            writer.Write("<link href=\"");
            writer.Write(include);
            writer.WriteLine("\" />");
        }
    }
}
