<%@ Page Title="Home Page" Language="C#" AutoEventWireup="true" MasterPageFile="~/Site.Master" CodeBehind="dynamic_page.aspx.cs" Inherits="SystemWebUISample.Pages.DynamicPage" %>

<%@ Register Src="~/TestUserControl.ascx" TagPrefix="uc" TagName="TestUserControl" %>


<script runat="server">

    protected void Page_Load(object sender, EventArgs e) {  
        //if (!Page.IsPostBack) {  
            BindData();  
       // }  
    } 
    protected void Page_PreInit(object sender, EventArgs e)
    {
    }

    protected void Button1_Click(object sender, EventArgs e)
    {
        label2.Text = GetText("Button1");
    }

    protected void Button2_Click(object sender, EventArgs e)
    {
        label2.Text = GetText("Button2");
    }


    protected void Grid_PageIndexChanged(object source, DataGridPageChangedEventArgs e) {  
        Grid.CurrentPageIndex = e.NewPageIndex;  
        BindData();  
    }  
   
</script>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
  <asp:Label id="label" Text="First Name" runat="server" />
  <asp:TextBox id="txt" runat="server" Value="" />

  <hr />

  <asp:Button id="button1" value="Go" runat="server" Text="Click Me" OnClick="Button1_Click" />
  <asp:Button id="button2" value="Go" runat="server" Text="Click Me" OnClick="Button2_Click" />

  Button clicked: <b><asp:Label id="label2" Text="None" runat="server" /></b>

<br/><br/><br/><br/>

               <asp:DataGrid ID="Grid" runat="server" PageSize="5" AllowPaging="True" DataKeyField="EmpId" AutoGenerateColumns="False" CellPadding="4" ForeColor="#333333" GridLines="None" OnPageIndexChanged="Grid_PageIndexChanged" >  
                    <Columns>  
                        <asp:BoundColumn HeaderText="EmployeeId" DataField="EmpId"> </asp:BoundColumn>  
                        <asp:BoundColumn HeaderText="FirstName" DataField="FirstName"> </asp:BoundColumn>  
                        <asp:BoundColumn HeaderText="LastName" DataField="LastName"> </asp:BoundColumn>  
                        <asp:BoundColumn DataField="City" HeaderText="City"> </asp:BoundColumn>  
                        <asp:BoundColumn DataField="Email" HeaderText="Email"> </asp:BoundColumn>  
                        <asp:BoundColumn DataField="DateOfJoining" HeaderText="DateOfJoining"> </asp:BoundColumn>  
                    </Columns>  
                    <FooterStyle BackColor="#990000" Font-Bold="True" ForeColor="White" />  
                    <SelectedItemStyle BackColor="#FFCC66" Font-Bold="True" ForeColor="Navy" />  
                    <PagerStyle BackColor="#FFCC66" ForeColor="#333333" HorizontalAlign="Center" Mode="NumericPages" />  
                    <AlternatingItemStyle BackColor="White" />  
                    <ItemStyle BackColor="#FFFBD6" ForeColor="#333333" />  
                    <HeaderStyle BackColor="#990000" Font-Bold="True" ForeColor="White" /> </asp:DataGrid> <br /> <br />

       <div class="col-md-4">
            <section id="socialLoginForm">
                <uc:TestUserControl runat="server" ID="OpenAuthLogin" />
            </section>
        </div>


<p>Click on an item in the list to raise the Click event.</p> 
        
    <asp:BulletedList id="ItemsBulletedList" 
      BulletStyle="Disc"
      DisplayMode="LinkButton"
      OnClick="ItemsBulletedList_Click"
      runat="server">
        <asp:ListItem Value="http://www.cohowinery.com">Coho Winery</asp:ListItem>
        <asp:ListItem Value="http://www.contoso.com">Contoso, Ltd.</asp:ListItem>
        <asp:ListItem Value="http://www.tailspintoys.com">Tailspin Toys</asp:ListItem>
    </asp:BulletedList>
            
    <asp:Label id="Message" 
      Font-Size="12"
      Width="168px" 
      Font-Bold="True" 
      runat="server"
      AssociatedControlID="ItemsBulletedList"/>
</asp:Content>
