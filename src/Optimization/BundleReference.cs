// MIT License.

using System.Web.UI;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

        var options = Context.GetRequiredService<IOptions<BundleReferenceOptions>>().Value;

        if (!options.Bundles.TryGetBundle(Path, out var bundle))
        {
            Logger.LogWarning("Unknown requested bundle for {Path}", Path);
        }
        else if (bundle is StyleBundle styles)
        {
            Write(writer, styles);
        }
        else if (bundle is ScriptBundle scripts)
        {
            Write(writer, scripts);
        }
        else
        {
            Logger.LogWarning("Unknown requested bundle for {Path} {Type}", Path, bundle.GetType().Name);
        }
    }

    private void Write(HtmlTextWriter writer, ScriptBundle bundle)
    {
        foreach (var include in bundle.Paths)
        {
            writer.Write("<script type=\"text/json\" src=\"");
            writer.Write(ResolveUrl(include));
            writer.WriteLine("\"></script>");
        }
    }

    private void Write(HtmlTextWriter writer, StyleBundle bundle)
    {
        foreach (var include in bundle.Paths)
        {
            writer.Write("<link href=\"");
            writer.Write(ResolveUrl(include));
            writer.WriteLine("\" rel=\"stylesheet\" />");
        }
    }
}
