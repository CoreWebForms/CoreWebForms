<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TestUserControl.ascx.cs" Inherits="SystemWebUISample.TestUserControl" %>
<%@ Register Src="~/InsideTestUserControl.ascx" TagPrefix="ucInside" TagName="InnerTestUserControl" %>
<div>
     <%=HeaderText %>
    <ucInside:InnerTestUserControl runat="server" ID="InnerOpenAuthLogin" />
</div>
