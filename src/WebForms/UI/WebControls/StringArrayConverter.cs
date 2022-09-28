// MIT License.

using System.ComponentModel;
using System.Globalization;

namespace System.Web.UI.WebControls;

// 

/// <devdoc>
///    <para>Converts a string separated by commas to and from 
///       an array of strings.</para>
/// </devdoc>
public class StringArrayConverter : TypeConverter
{

    /// <devdoc>
    ///    <para>Determines if the specified data type can be converted to an array of strings. </para>
    /// </devdoc>
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    {
        if (sourceType == typeof(string))
        {
            return true;
        }
        return false;
    }

    /// <devdoc>
    ///    <para>Parses a string  separated by 
    ///       commas into an array of strings. </para>
    /// </devdoc>
    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
        if (value is string)
        {
            if (((string)value).Length == 0)
            {
                return Array.Empty<string>();
            }

            // hard code comma, since it is persisted to HTML
            // 
            string[] names = ((string)value).Split(new char[] { ',' });
            for (int i = 0; i < names.Length; i++)
            {
                names[i] = names[i].Trim();
            }
            return names;
        }
        throw GetConvertFromException(value);
    }

    /// <devdoc>
    ///    <para> Creates a string separated 
    ///       by commas from an array of strings.</para>
    /// </devdoc>
    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
    {
        if (destinationType == typeof(string))
        {

            if (value == null)
            {
                return String.Empty;
            }
            // hard code comma, since it is persisted to HTML
            // 
            return string.Join(",", ((string[])value));
        }
        throw GetConvertToException(value, destinationType);
    }
}
