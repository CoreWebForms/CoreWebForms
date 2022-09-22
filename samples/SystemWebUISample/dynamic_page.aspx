<%@ Page Title="About" Language="C#" MasterPageFile="~/Site.Master" Inherits="SystemWebUISample.Pages.DynamicPage" AutoEventWireup="true" CodeBehind="dynamic_page.aspx.cs"  %>

<h1>Edit3</h1>

<br />

<script runat="server">
    protected void Page_PreInit(object sender, EventArgs e)
    {
    }

    protected void Button1_Click(object sender, EventArgs e)
    {
        label2.Text = "Button1";
    }

    protected void Button2_Click(object sender, EventArgs e)
    {
        label2.Text = "Button2";
    }
</script>
  
<form id="frm" runat="server">
    <asp:Label id="label" Text="First Name" runat="server" />
    <asp:TextBox id="txt" runat="server" Value="Leave it alone" />

    <hr />

    <asp:Button id="button1" value="Go" runat="server" Text="Click Me" OnClick="Button1_Click" />
    <asp:Button id="button2" value="Go" runat="server" Text="Click Me" OnClick="Button2_Click" />

    Button clicked: <b><asp:Label id="label2" Text="None" runat="server" /></b>
   
<!-- foo -->
         
<%--
     <asp:RequiredFieldValidator runat="server" ControlToValidate="Name" Display="Dynamic"
            CssClass="field-validation-valid text-danger" ErrorMessage="The Name field is required." />
--%>
    

</form>
