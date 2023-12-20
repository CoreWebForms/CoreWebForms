// MIT License.
using System.Web.UI;
using System;
using System.ComponentModel;
using System.Collections.Generic;

namespace SystemWebUISample;

public partial class ABTestUserControl : System.Web.UI.UserControl
{
    protected void Page_Load(object sender, EventArgs e)
    {/*
            if (IsPostBack)
            {
                var provider = Request.Form["provider"];
                if (provider == null)
                {
                    return;
                }
                // Request a redirect to the external login provider
                string redirectUrl = ResolveUrl(String.Format(CultureInfo.InvariantCulture, "~/Account/RegisterExternalLogin?{0}={1}&returnUrl={2}", IdentityHelper.ProviderNameKey, provider, ReturnUrl));
                var properties = new AuthenticationProperties() { RedirectUri = redirectUrl };
                // Add xsrf verification when linking accounts
                if (Context.User.Identity.IsAuthenticated)
                {
                    properties.Dictionary[IdentityHelper.XsrfKey] = Context.User.Identity.GetUserId();
                }
                Context.GetOwinContext().Authentication.Challenge(properties, provider);
                Response.StatusCode = 401;
                Response.End();
            }
            */
    }

    public string ReturnUrl { get; set; }

    public IEnumerable<string> GetProviderNames()
    {
        List<String> providers = new List<string>();
        providers.Add("test1");
        providers.Add("test2");
        return providers;
    }

    protected void TestUserControl_Click(object sender, EventArgs e)
    {
       // lblForUserControl.Text = "clicked";
    }

}
