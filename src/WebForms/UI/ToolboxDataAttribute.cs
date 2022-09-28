// MIT License.

using System.Web.Util;

#nullable disable

namespace System.Web.UI;
/// <devdoc>
///     ToolboxDataAttribute 
/// </devdoc>
[AttributeUsage(AttributeTargets.Class)]
public sealed class ToolboxDataAttribute : Attribute
{
    public static readonly ToolboxDataAttribute Default = new ToolboxDataAttribute(string.Empty);

    /// <devdoc>
    /// </devdoc>
    public string Data { get; } = string.Empty;

    /// <devdoc>
    /// </devdoc>
    /// <internalonly/>
    public ToolboxDataAttribute(string data)
    {
        Data = data;
    }

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public override int GetHashCode() => Data?.GetHashCode() ?? 0;

    /// <devdoc>
    /// </devdoc>
    /// <internalonly/>
    public override bool Equals(object obj)
    {
        if (obj == this)
        {
            return true;
        }

        return (obj != null) && (obj is ToolboxDataAttribute attribute) ? StringUtil.EqualsIgnoreCase(attribute.Data, Data) : false;
    }

    /// <devdoc>
    /// </devdoc>
    /// <internalonly/>
    public override bool IsDefaultAttribute()
    {
        return Equals(Default);
    }
}
