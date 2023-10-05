// MIT License.

#nullable disable

using System.Web.UI.WebControls;

namespace System.Web.UI;

internal static class StyleHtmlTextWriterExtensions
{
    public static void EnterStyle(this HtmlTextWriter writer, Style style, HtmlTextWriterTag tag)
    {
        if (!style.IsEmpty || tag != HtmlTextWriterTag.Span)
        {
            style.AddAttributesToRender(writer);
            writer.RenderBeginTag(tag);
        }
    }

    public static void ExitStyle(this HtmlTextWriter writer, Style style, HtmlTextWriterTag tag)
    {
        // Review: This requires that the style doesn't change between beginstyle/endstyle.
        if (!style.IsEmpty || tag != HtmlTextWriterTag.Span)
        {
            writer.RenderEndTag();
        }
    }

    public static void EnterStyle(HtmlTextWriter writer, Style style)
    {
        writer.EnterStyle(style, HtmlTextWriterTag.Span);
    }

    public static void ExitStyle(this HtmlTextWriter writer, Style style)
    {
        writer.ExitStyle(style, HtmlTextWriterTag.Span);
    }
}
