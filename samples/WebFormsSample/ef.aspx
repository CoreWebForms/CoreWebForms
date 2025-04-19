<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ef.aspx.cs" Inherits="SystemWebUISample.Pages.ef" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<script runat="server">
    protected void Page_Load(object sender, EventArgs e) {
       if (!Page.IsPostBack) {  
           CallDB();
       }
    }
</script>

<html xmlns="http://www.w3.org/1999/xhtml" >
<body>
<form runat="server">
 <asp:TextBox id="txt" runat="server" />
    <p>
        Entity Framework Test.
    </p>
</form>
</body>

</html>
