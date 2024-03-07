using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

public partial class _Default : System.Web.UI.Page 
{
    protected void Page_Load(object sender, EventArgs e)
    {
        welcomeWebPart.Text = TextBoxName.Text;
        if (IsPostBack == false)
        {
            foreach (WebPartDisplayMode mode in WebPartManager1.SupportedDisplayModes)
            {
                DropDownList1.Items.Add(mode.Name);
            }

            DropDownList1.SelectedValue = WebPartManager1.DisplayMode.ToString();            
        }
    }
    protected void DropDownList1_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (WebPartManager1.SupportedDisplayModes[DropDownList1.SelectedValue] != null)
        {
            WebPartManager1.DisplayMode = WebPartManager1.SupportedDisplayModes[DropDownList1.SelectedValue];
        }
    }        
}
