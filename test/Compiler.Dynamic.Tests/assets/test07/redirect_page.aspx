<%@ Page Title="Home Page" Language="C#" AutoEventWireup="true" %> 

<script runat="server">
    protected void Page_Load(object sender, EventArgs e)
    { 
        Context.Response.Redirect("~/actual.aspx");
    }
</script>

Shouldn't show this!

