<%@ Page Title="About" Language="C#" MasterPageFile="~/Site.Master" Inherits="SystemWebUISample.Pages.MyPage" AutoEventWireup="true" CodeBehind="About.aspx.cs"  %>

<h1>Edit3</h1>

<br />

<script runat="server">
    protected void Page_PreInit(object sender, EventArgs e) {
        System.Diagnostics.Debugger.Break();
    }

    protected void Button1_Click(object sender, EventArgs e)
    {
        System.Diagnostics.Debugger.Break();
    }
</script>
  
<form id="frm" runat="server">
    <asp:Label ID="txt" Text="First Name" runat="server" />
    <asp:TextBox id="txt" runat="server" Value="Leave it alone" />
   
<!-- foo -->
         
<%--
     <asp:RequiredFieldValidator runat="server" ControlToValidate="Name" Display="Dynamic"
            CssClass="field-validation-valid text-danger" ErrorMessage="The Name field is required." />
--%>
    
    <asp:Button id="button1" value="Go" runat="server" Text="Click Me" OnClick="Button1_Click" />
    <asp:Button id="button2" value="Go" runat="server" Text="Click Me" OnClick="Button2_Click" />

</form>
