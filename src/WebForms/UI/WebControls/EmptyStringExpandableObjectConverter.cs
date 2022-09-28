// MIT License.

using System.ComponentModel;
using System.Globalization;

#nullable disable

namespace System.Web.UI.WebControls;
/// <devdoc>
/// Converts an object to String.Empty so it looks better in the designer property grid.
/// </devdoc>
internal sealed class EmptyStringExpandableObjectConverter : ExpandableObjectConverter
{

    /// <devdoc>
    /// Returns String.Empty so the object looks better in the designer property grid.
    /// </devdoc>
    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
    {
        return destinationType == typeof(string) ? (object)string.Empty : throw GetConvertToException(value, destinationType);
    }
}

