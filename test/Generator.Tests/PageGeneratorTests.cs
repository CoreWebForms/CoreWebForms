// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.SystemWebAdapters.UI.Generator;
using Xunit;

using VerifyCS = Microsoft.AspNetCore.SystemWebAdapters.UI.Generator.Tests.GeneratorVerifier<
    Microsoft.AspNetCore.SystemWebAdapters.UI.Generator.PageGenerator>;

namespace SystemWebAdapters.UI.Generator.Tests;

public class PageGeneratorTests
{
    private static readonly string NewLine = Environment.NewLine.Replace("\r", "\\r").Replace("\n", "\\n");

    [Fact]
    public async Task Empty()
    {
        await new VerifyCS.Test().RunAsync().ConfigureAwait(false);
    }

    [Fact]
    public async Task EmptyAspx()
    {
        const string BaseClass = @"namespace WebApplication12
{
    public class About : global::System.Web.UI.Page
    {
    }
}";
        var aspx = "<%@ Page Title=\"About\" Language=\"C#\" MasterPageFile=\"~/Site.Master\" AutoEventWireup=\"true\" CodeBehind=\"About.aspx.cs\" Inherits=\"WebApplication12.About\" %>\r\n";
        var generated = @"[Microsoft.AspNetCore.SystemWebAdapters.UI.AspxPageAttribute(""/page.aspx"")]
internal partial class _page_aspx : WebApplication12.About
{
    protected override void InitializeComponents()
    {
    }
}
";

        await new VerifyCS.Test()
        {
            TestState =
            {
                Sources =
                {
                    ("/about.cs", BaseClass),
                },
                AdditionalFiles =
                {
                    ("/page.aspx", aspx),
                },
                GeneratedSources =
                {
                    (typeof(PageGenerator), "page.aspx.g.cs", generated),
                },
            },
        }.RunAsync().ConfigureAwait(false);
    }

    [Fact]
    public async Task JustHtml()
    {
        const string BaseClass = @"namespace WebApplication12
{
    public class About : global::System.Web.UI.Page
    {
    }
}";
        var aspx = @"<%@ Page Title=""About"" Language=""C#"" MasterPageFile=""~/Site.Master"" AutoEventWireup=""true"" CodeBehind=""About.aspx.cs"" Inherits=""WebApplication12.About"" %>
<div />
";
        var generated = @$"[Microsoft.AspNetCore.SystemWebAdapters.UI.AspxPageAttribute(""/page.aspx"")]
internal partial class _page_aspx : WebApplication12.About
{{
    protected override void InitializeComponents()
    {{
        var control_1 = new global::System.Web.UI.LiteralControl(""<div />"");
        Controls.Add(control_1);
        var control_2 = new global::System.Web.UI.LiteralControl(""{NewLine}"");
        Controls.Add(control_2);
    }}
}}
";

        await new VerifyCS.Test()
        {
            TestState =
            {
                Sources =
                {
                    ("/about.cs", BaseClass),
                },
                AdditionalFiles =
                {
                    ("/page.aspx", aspx),
                },
                GeneratedSources =
                {
                    (typeof(PageGenerator), "page.aspx.g.cs", generated),
                },
            },
        }.RunAsync().ConfigureAwait(false);
    }

    [Fact]
    public async Task DivWithRunatId()
    {
        const string BaseClass = @"namespace WebApplication12
{
    public class About : global::System.Web.UI.Page
    {
    }
}";
        var aspx = @"<%@ Page Title=""About"" Language=""C#"" MasterPageFile=""~/Site.Master"" AutoEventWireup=""true"" CodeBehind=""About.aspx.cs"" Inherits=""WebApplication12.About"" %>
<div runat=""server"" id=""hi"" />
";
        var generated = @$"[Microsoft.AspNetCore.SystemWebAdapters.UI.AspxPageAttribute(""/page.aspx"")]
internal partial class _page_aspx : WebApplication12.About
{{
    protected override void InitializeComponents()
    {{
        var control_1 = new global::System.Web.UI.HtmlControls.HtmlGenericControl(""div"");
        control_1.Id = ""hi"";
        Controls.Add(control_1);
        var control_2 = new global::System.Web.UI.LiteralControl(""{NewLine}"");
        Controls.Add(control_2);
    }}
}}
";

        await new VerifyCS.Test()
        {
            TestState =
            {
                Sources =
                {
                    ("/about.cs", BaseClass),
                },
                AdditionalFiles =
                {
                    ("/page.aspx", aspx),
                },
                GeneratedSources =
                {
                    (typeof(PageGenerator), "page.aspx.g.cs", generated),
                },
            },
        }.RunAsync().ConfigureAwait(false);
    }

    [Fact]
    public async Task TextBoxInContent()
    {
        const string BaseClass = @"namespace WebApplication12
{
    public class About : global::System.Web.UI.Page
    {
    }
}";
        var aspx = @"<%@ Page Title=""About"" Language=""C#"" MasterPageFile=""~/Site.Master"" AutoEventWireup=""true"" CodeBehind=""About.aspx.cs"" Inherits=""WebApplication12.About"" %>
<form id=""frm"" runat=""server"">
    <asp:TextBox id=""txt"" runat=""server"" />
</form>
";
        var generated = @$"[Microsoft.AspNetCore.SystemWebAdapters.UI.AspxPageAttribute(""/page.aspx"")]
internal partial class _page_aspx : WebApplication12.About
{{
    protected override void InitializeComponents()
    {{
        var control_1 = new global::System.Web.UI.HtmlControls.HtmlForm();
        control_1.Id = ""frm"";
        Controls.Add(control_1);
        {{
            var control_1_1 = new global::System.Web.UI.LiteralControl(""{NewLine}    "");
            control_1.Controls.Add(control_1_1);
            var control_1_2 = new global::System.Web.UI.WebControls.TextBox();
            control_1_2.Id = ""txt"";
            control_1.Controls.Add(control_1_2);
            var control_1_3 = new global::System.Web.UI.LiteralControl(""{NewLine}"");
            control_1.Controls.Add(control_1_3);
        }}
        var control_2 = new global::System.Web.UI.LiteralControl(""{NewLine}"");
        Controls.Add(control_2);
    }}
}}
";

        await new VerifyCS.Test()
        {
            TestState =
            {
                Sources =
                {
                    ("/about.cs", BaseClass),
                },
                AdditionalFiles =
                {
                    ("/page.aspx", aspx),
                },
                GeneratedSources =
                {
                    (typeof(PageGenerator), "page.aspx.g.cs", generated),
                },
            },
        }.RunAsync().ConfigureAwait(false);
    }
}
