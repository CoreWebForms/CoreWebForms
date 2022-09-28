// MIT License.

using System.ComponentModel;

/*
 */
namespace System.Web;
/// <devdoc>
///     DescriptionAttribute marks a property, event, or extender with a
///     description. Visual designers can display this description when referencing
///     the member.
/// </devdoc>
[AttributeUsage(AttributeTargets.All)]
internal sealed class WebSysDescriptionAttribute : DescriptionAttribute
{

    private bool replaced;

    /// <devdoc>
    ///    <para>Constructs a new sys description.</para>
    /// </devdoc>
    internal WebSysDescriptionAttribute(string description) : base(description)
    {
    }

    /// <devdoc>
    ///    <para>Retrieves the description text.</para>
    /// </devdoc>
    public override string Description
    {
        get
        {
            if (!replaced)
            {
                replaced = true;
                DescriptionValue = SR.GetString(base.Description);
            }
            return base.Description;
        }
    }

    public override object TypeId => typeof(DescriptionAttribute);
}
