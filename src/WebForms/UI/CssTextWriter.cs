// MIT License.

using System.Reflection;

namespace System.Web.UI;

internal sealed class CssTextWriter
{
    private static readonly Lazy<Action<TextWriter, HtmlTextWriterStyle, string, string>> _writeAttribute = new(() =>
    {
        var type = typeof(HtmlTextWriter).Assembly.GetType("System.Web.UI.CssTextWriter");
        var method = type.GetMethod("WriteAttribute", BindingFlags.Static | BindingFlags.NonPublic, new Type[] { typeof(TextWriter), typeof(HtmlTextWriterStyle), typeof(string), typeof(string) });

        if (method is null)
        {
            throw new InvalidOperationException("WriteAttribute is currently required inside the internal class System.Web.UI.CssTextWriter");
        }

        return method.CreateDelegate<Action<TextWriter, HtmlTextWriterStyle, string, string>>();
    });

    private readonly TextWriter _other;

    public CssTextWriter(TextWriter other)
    {
        _other = other;
    }

    private sealed class Accessor : HtmlTextWriter
    {
        private static readonly Accessor _accessor = new(Null);

        public Accessor(TextWriter writer)
            : base(writer)
        {
        }

        public static HtmlTextWriterStyle GetStyleKeyInternal(string key) => _accessor.GetStyleKey(key);

        public static string GetStyleNameInternal(HtmlTextWriterStyle style) => _accessor.GetStyleName(style);
    }

    internal static HtmlTextWriterStyle GetStyleKey(string key) => Accessor.GetStyleKeyInternal(key);

    internal static string GetStyleName(HtmlTextWriterStyle style) => Accessor.GetStyleNameInternal(style);

    internal void WriteAttribute(string key, string value)
        => _writeAttribute.Value(_other, GetStyleKey(key), key, value);

    internal void WriteAttribute(HtmlTextWriterStyle key, string value)
        => _writeAttribute.Value(_other, key, GetStyleName(key), value);

    internal void WriteBeginCssRule(string selector)
    {
        _other.Write(selector);
        _other.Write(" { ");
    }

    internal void WriteEndCssRule()
        => _other.WriteLine(" }");
}
