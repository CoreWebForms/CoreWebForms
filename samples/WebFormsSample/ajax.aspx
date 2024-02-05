<%@ Page Async="true" Language="C#" AutoEventWireup="true"  %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
	<title>Ajax Test</title>
	<style>
		* { font-family: Consolas; }
		body { background-color: #222; color: #eee;  }
	</style>
</head>
<body>
<form id="form1" runat="server">

	<script runat="server">
		protected void Button1_Click(object sender, EventArgs e) {
			Label1.Text = "I am postyback!";
		}
	</script>

	<p>Should stay GET after click: <b><% Response.Write(Page.Request.HttpMethod); %> </b></p>

	<asp:ScriptManager EnablePartialRendering="true" ID="ScriptManager1" runat="server"></asp:ScriptManager>
	
	<asp:UpdatePanel ID="UpdatePanel1" runat="server">
		<ContentTemplate>
			<asp:Label ID="Label1" runat="server" Text="Do ->"></asp:Label>
			<asp:Button ID="Button1" runat="server" Text="Click" OnClick="Button1_Click" />
		</ContentTemplate>
	</asp:UpdatePanel>
	
</form>
</body>
</html>
