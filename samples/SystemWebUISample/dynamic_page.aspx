<%@ Page Title="About" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="About.aspx.cs"  %>

<h1>Edit3</h1>

<br />

<form id="frm" runat="server">
    <asp:Label ID="txt" Text="First Name" runat="server" />
    <asp:TextBox id="txt" runat="server" Value="Leave it alone" />

<!-- foo -->

<%--
     <asp:RequiredFieldValidator runat="server" ControlToValidate="Name" Display="Dynamic"
            CssClass="field-validation-valid text-danger" ErrorMessage="The Name field is required." />

    <asp:Button id="button1" value="Go" />
--%>

</form>
