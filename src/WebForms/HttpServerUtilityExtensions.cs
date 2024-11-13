// MIT License.

namespace System.Web;

public static class HttpServerUtilityExtensions
{
    public static string HtmlEncode(this HttpServerUtility server, string s)
        => HttpUtility.HtmlEncode(s);

    public static string HtmlDecode(this HttpServerUtility server, string s)
        => HttpUtility.HtmlDecode(s);
}
