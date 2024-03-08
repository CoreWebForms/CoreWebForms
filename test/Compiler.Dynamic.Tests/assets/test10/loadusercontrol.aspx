<%@ page language="C#" autoeventwireup="true" %>

<%@ register src="~/mycontrol.ascx" tagprefix="mine" tagname="mycontrol" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<body>
    <form runat="server">
        <asp:Panel ID="panel1" runat="Server">
        </asp:Panel>
    </form>

</body>
</html>

<script runat="server">
    protected void Page_Load(object sender, EventArgs e)
    {
        panel1.Controls.Add(Page.LoadControl("~/mycontrol.ascx"));
    }     
</script>
