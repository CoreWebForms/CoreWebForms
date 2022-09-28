// MIT License.

namespace Microsoft.AspNetCore.SystemWebAdapters.Compiler;

internal readonly struct QName
{
    public QName(string ns, string name)
    {
        Namespace = ns;
        Name = name;
    }

    public string Namespace { get; }

    public string Name { get; }
}
