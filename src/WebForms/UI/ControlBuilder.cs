// MIT License.

using System.Collections;

/*
 */
namespace System.Web.UI;
[AttributeUsage(AttributeTargets.Property)]
internal sealed class IgnoreUnknownContentAttribute : Attribute
{
    internal IgnoreUnknownContentAttribute() { }
}

public abstract class ControlBuilder
{
    public virtual bool HasBody() => true;

    public virtual Type GetChildControlType(string tagName, IDictionary attribs) => null;

    public virtual bool AllowWhitespaceLiterals() => true;
}
