<%@ Page Language="C#"  %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">

</script>

<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">
  <title>Simple Navigation Controls using SiteMap</title>
</head>
<body>
  <form id="form1" runat="server">
  <div>
  <asp:SiteMapDataSource ID="SiteMapDataSource1" Runat="server" />

  <h2>Using a Horizontal Menu</h2>
  <asp:Menu ID="Menu1" Runat="server" DataSourceID="SiteMapDataSource1"
    Orientation="Horizontal" 
    StaticDisplayLevels="2" >
  </asp:Menu>

  </div>
  </form>
</body>
</html>
