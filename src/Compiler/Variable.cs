// MIT License.

namespace Microsoft.AspNetCore.SystemWebAdapters.Compiler;

internal readonly struct Variable
{
    public Variable(string name, QName qname)
    {
        Name = name;
        Type = qname;
    }

    public string Name { get; }

    public QName Type { get; }
}
