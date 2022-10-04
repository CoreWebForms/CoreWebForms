// MIT License.

using System;
using System.Web.UI;

namespace SystemWebUISample;

public partial class SiteMaster : MasterPage
{
    protected void Login_Click(object sender, EventArgs e)
    {
        //// Send an OpenID Connect sign-in request.
        //if (!Request.IsAuthenticated)
        //{
        //    Context.GetOwinContext().Authentication.Challenge(new AuthenticationProperties { RedirectUri = "/" }, OpenIdConnectAuthenticationDefaults.AuthenticationType);
        //}
    }
}
