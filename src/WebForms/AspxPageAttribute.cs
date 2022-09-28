// MIT License.

namespace Microsoft.AspNetCore.SystemWebAdapters.UI;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class AspxPageAttribute : Attribute
{
    public AspxPageAttribute(string path)
    {
        Path = path;
    }

    public string Path { get; }
}
