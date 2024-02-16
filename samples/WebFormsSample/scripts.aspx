<%@ Page Async="true" Language="C#" AutoEventWireup="true"  %>

<!DOCTYPE html>

<html lang="en">

<head runat="server">
    <asp:PlaceHolder runat="server">
        <%: Scripts.Render("~/scriptbundle") %>
    </asp:PlaceHolder>
</head>

<body>
    <form runat="server">
        <asp:ScriptManager runat="server">
            <Scripts>
                <asp:ScriptReference Name="WebForms.js" Assembly="System.Web" Path="~/Scripts/WebForms/WebForms.js" />
            </Scripts>
        </asp:ScriptManager>
   </form>
</body>
