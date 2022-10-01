// MIT License.

namespace System.Web.Services;

internal class ILoginProvider
{
    internal void OnAuthenticated(string userNameInternal, bool rememberMeSet, string redirectUrl)
    {
        throw new NotImplementedException();
    }

    internal void RedirectToLoginPage(string v)
    {
        throw new NotImplementedException();
    }
}
