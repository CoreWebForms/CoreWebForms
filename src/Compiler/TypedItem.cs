// MIT License.

using Microsoft.AspNetCore.SystemWebAdapters.Compiler.ParserImpl;

namespace Microsoft.AspNetCore.SystemWebAdapters.Compiler;

internal readonly struct TypedItem
{
    public TypedItem(QName qname, TagAttributes attributes)
    {
        QName = qname;
        Attributes = attributes;
    }

    public QName QName { get; }

    public TagAttributes Attributes { get; }
}
