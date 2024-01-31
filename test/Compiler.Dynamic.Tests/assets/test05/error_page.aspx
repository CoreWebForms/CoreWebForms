<%@ Page Title="Home Page" Language="C#" AutoEventWireup="true" %>

<script runat="server">
      string SomeText = "hello world!"
</script>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
  <%=SomeText %>
</asp:Content>
