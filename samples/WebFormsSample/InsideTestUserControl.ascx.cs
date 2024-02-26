// MIT License.

using System.Web.UI;
using System;
using System.ComponentModel;
using System.Collections.Generic;

namespace SystemWebUISample;

public partial class InsideTestUserControl : System.Web.UI.UserControl
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            lblForPostBackInner.Text = "Not Postback";
        }
        else
        {
            lblForPostBackInner.Text = "Yes Postback";
        }

    }
}
