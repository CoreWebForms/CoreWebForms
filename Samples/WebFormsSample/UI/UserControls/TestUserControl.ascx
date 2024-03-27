<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TestUserControl.ascx.cs" Inherits="SystemWebUISample.TestUserControl" %>
<%@ Register Src="~/InsideTestUserControl.ascx" TagPrefix="ucInside" TagName="InnerTestUserControl" %>

<div id="socialLoginList">
    <h4>Use another service to log in.</h4>
    <hr />
    <asp:ListView runat="server" ID="providerDetails" ItemType="System.String"
        SelectMethod="GetProviderNames" ViewStateMode="Disabled">
        <ItemTemplate>
            <p>
                <button type="submit" class="btn btn-default" name="provider" value="<%#: Item %>"
                    title="Log in using your <%#: Item %> account.">
                    <%#: Item %>
                </button>
            </p>
        </ItemTemplate>
        <EmptyDataTemplate>
            <div>
                <p>There are no external authentication services configured. See <a href="http://go.microsoft.com/fwlink/?LinkId=252803">this article</a> for details on setting up this ASP.NET application to support logging in via external services.</p>
            </div>
        </EmptyDataTemplate>
    </asp:ListView>
   <asp:Button id="ITestUserControl" value="ITestUserControl" runat="server" Text="Click Me" OnClick="ITestUserControl_Click" />
<asp:Label id="lblForUserControl" Text="UC Name" runat="server" />
<asp:Label id="lblForPostBack" Text="" runat="server" />
       <div class="col-inner-md-4">
            <section id="innersocialLoginForm">
                <ucInside:InnerTestUserControl runat="server" ID="InnerOpenAuthLogin" />
            </section>
        </div>
</div>
