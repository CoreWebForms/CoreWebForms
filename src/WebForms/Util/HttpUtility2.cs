// MIT License.

using System.Globalization;

namespace System.Web.Util;

internal class HttpUtility2
{
    internal static bool TryParseCoordinates(string value, out double doubleValue)
    {
        var flags = NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite | NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign;
        return Double.TryParse(value, flags, CultureInfo.InvariantCulture, out doubleValue);
    }
}
