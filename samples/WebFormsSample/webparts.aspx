<%@ Page Language="C#" AutoEventWireup="true"  CodeFile="webparts.aspx.cs" Inherits="_Default" %>

<!-- Originallly from https://www.codeproject.com/Articles/337551/A-Beginners-Tutorial-on-ASP-NET-Web-Parts -->

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>WebParts sample</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:WebPartManager ID="WebPartManager1" runat="server">
        </asp:WebPartManager>
        &nbsp;</div>
        <asp:WebPartZone ID="WebPartZoneHeader" runat="server" Height="1px" Width="865px" HeaderText="Welcome">
            <ZoneTemplate>
                <asp:Label ID="welcomeWebPart" runat="server" Text="User" title="Welcome" Width="199px"></asp:Label>
            </ZoneTemplate>
        </asp:WebPartZone>
        <asp:WebPartZone ID="WebPartZoneContent" runat="server" Height="1px" Width="865px" HeaderText="Pick a Day">
            <ZoneTemplate>
                 <asp:TextBox ID="TextBoxName" runat="server" Title="Enter your name"></asp:TextBox>
                <asp:DropDownList ID="DropDownList1" runat="server" Title="Change Display modes" AutoPostBack="True" OnSelectedIndexChanged="DropDownList1_SelectedIndexChanged"></asp:DropDownList>
                <asp:Calendar ID="CalendarWebPArt" runat="server" title="Pick a day"></asp:Calendar>                               
            </ZoneTemplate>
        </asp:WebPartZone>
        <asp:CatalogZone ID="CatalogZone1" runat="server">
            <ZoneTemplate>
                <asp:PageCatalogPart ID="PageCatalogPart1" runat="server" />
            </ZoneTemplate>
        </asp:CatalogZone>
        <asp:WebPartZone ID="WebPartZoneFooter" runat="server" Height="35px" Width="865px" HeaderText="Copyright">
            <ZoneTemplate>
                <asp:Label ID="footerWebPart" runat="server" Text="This is a test website." title="Copyright info"></asp:Label>
            </ZoneTemplate>
        </asp:WebPartZone>
    </form>
</body>
</html>
