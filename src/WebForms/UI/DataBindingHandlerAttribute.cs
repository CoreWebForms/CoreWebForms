// MIT License.

#nullable disable

namespace System.Web.UI;
/// <devdoc>
/// </devdoc>
[AttributeUsage(AttributeTargets.Class)]
public sealed class DataBindingHandlerAttribute : Attribute
{
    private readonly string _typeName;

    /// <devdoc>
    /// </devdoc>
    public static readonly DataBindingHandlerAttribute Default = new DataBindingHandlerAttribute();

    /// <devdoc>
    /// </devdoc>
    public DataBindingHandlerAttribute()
    {
        _typeName = string.Empty;
    }

    /// <devdoc>
    /// </devdoc>
    public DataBindingHandlerAttribute(Type type)
    {
        _typeName = type.AssemblyQualifiedName;
    }

    /// <devdoc>
    /// </devdoc>
    public DataBindingHandlerAttribute(string typeName)
    {
        _typeName = typeName;
    }

    /// <devdoc>
    /// </devdoc>
    public string HandlerTypeName => _typeName ?? string.Empty;

    /// <internalonly/>
    public override bool Equals(object obj)
    {
        if (obj == this)
        {
            return true;
        }

        return obj is DataBindingHandlerAttribute other
            ? string.Compare(HandlerTypeName, other.HandlerTypeName,
                                   StringComparison.Ordinal) == 0
            : false;
    }

    /// <internalonly/>
    public override int GetHashCode()
    {
        return HandlerTypeName.GetHashCode();
    }
}

