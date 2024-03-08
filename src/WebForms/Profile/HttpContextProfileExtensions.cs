// MIT License.

namespace System.Web.Profile;

public static class HttpContextProfileExtensions
{
    public static ProfileInfo GetProfile(this HttpContext context)
    {
        return new ProfileInfo(context.User.Identity?.Name, true, DateTime.Now, DateTime.Now, 0);
    }
}
