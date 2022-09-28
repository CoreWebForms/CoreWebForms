// MIT License.

#nullable disable

namespace System.Web.UI;
/// <devdoc>
///     LiteralContentAttribute indicates whether the contents within a tag representing
///     a custom/web control should be treated by Trident as a "literal/text" content.
///     Web controls supporting complex properties (like Templates, etc.) typically
///     mark themselves as "literals", thereby letting the designer infra-structure
///     and Trident deal with the persistence of those attributes.
///
///     If LiteralContentAttribute.No is present or no LiteralContentAttribute marking
///     exists, then the tag corresponding to the web control is not treated as a literal
///     content tag.
///     If LiteralContentAttribute.Yes is present, then the tag corresponding to the web
///     control is treated as a literal content tag.
/// </devdoc>
[AttributeUsage(AttributeTargets.All)]
public sealed class PersistenceModeAttribute : Attribute
{
    /// <devdoc>
    ///     This marks a property or event as persistable in the HTML tag as an attribute.
    /// </devdoc>
    public static readonly PersistenceModeAttribute Attribute = new PersistenceModeAttribute(PersistenceMode.Attribute);

    /// <devdoc>
    ///     This marks a property or event as persistable within the HTML tag as a nested tag.
    /// </devdoc>
    public static readonly PersistenceModeAttribute InnerProperty = new PersistenceModeAttribute(PersistenceMode.InnerProperty);

    /// <devdoc>
    ///     This marks a property or event as persistable within the HTML tag as a child.
    /// </devdoc>
    public static readonly PersistenceModeAttribute InnerDefaultProperty = new PersistenceModeAttribute(PersistenceMode.InnerDefaultProperty);

    /// <devdoc>
    ///     This marks a property or event as persistable within the HTML tag as a child.
    /// </devdoc>
    public static readonly PersistenceModeAttribute EncodedInnerDefaultProperty = new PersistenceModeAttribute(PersistenceMode.EncodedInnerDefaultProperty);

    /// <devdoc>
    /// </devdoc>
    public static readonly PersistenceModeAttribute Default = Attribute;

    /// <internalonly/>
    public PersistenceModeAttribute(PersistenceMode mode)
    {
        if (mode < PersistenceMode.Attribute || mode > PersistenceMode.EncodedInnerDefaultProperty)
        {
            throw new ArgumentOutOfRangeException(nameof(mode));
        }

        Mode = mode;
    }

    /// <devdoc>
    /// </devdoc>
    public PersistenceMode Mode { get; } = PersistenceMode.Attribute;

    /// <internalonly/>
    public override int GetHashCode() => Mode.GetHashCode();

    /// <devdoc>
    /// </devdoc>
    /// <internalonly/>
    public override bool Equals(object obj)
    {
        if (obj == this)
        {
            return true;
        }

        return obj is PersistenceModeAttribute attribute && attribute.Mode == Mode;
    }

    /// <devdoc>
    /// </devdoc>
    /// <internalonly/>
    public override bool IsDefaultAttribute() => Equals(Default);
}
