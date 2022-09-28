// MIT License.

using System.ComponentModel;
using System.Globalization;

#nullable disable

namespace System.Web.UI.WebControls;
/// <devdoc>
///   Converts a string with font names separated by commas to and from 
///   an array of strings containing individual names.
/// </devdoc>
public class FontNamesConverter : TypeConverter
{

    /// <devdoc>
    ///   Determines if the specified data type can be converted to an array of strings
    ///   containing individual font names.
    /// </devdoc>
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    {
        return sourceType == typeof(string);
    }

    /// <devdoc>
    ///   Parses a string that represents a list of font names separated by 
    ///   commas into an array of strings containing individual font names.
    /// </devdoc>
    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
        if (value is string)
        {
            if (((string)value).Length == 0)
            {
                return Array.Empty<string>();
            }

            string[] names = ((string)value).Split(new char[] { culture.TextInfo.ListSeparator[0] });
            for (int i = 0; i < names.Length; i++)
            {
                names[i] = names[i].Trim();
            }
            return names;
        }
        throw GetConvertFromException(value);
    }

    /// <devdoc>
    ///   Creates a string that represents a list of font names separated 
    ///   by commas from an array of strings containing individual font names.
    /// </devdoc>
    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
    {
        if (destinationType == typeof(string))
        {

            return value == null ? string.Empty : (object)string.Join(culture.TextInfo.ListSeparator, (string[])value);
        }
        throw GetConvertToException(value, destinationType);
    }
}
