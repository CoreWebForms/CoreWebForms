// MIT License.

namespace System.Web;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class PreApplicationStartMethodAttribute : Attribute
{
    public PreApplicationStartMethodAttribute(Type type, string methodName)
    {
        Type = type;
        MethodName = methodName;
    }

    public Type Type { get; }

    public string MethodName { get; }
}
