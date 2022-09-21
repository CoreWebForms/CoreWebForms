// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;
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
        var generated = @$"using System;
using System.Web;

[Microsoft.AspNetCore.SystemWebAdapters.UI.AspxPageAttribute(""/page.aspx"")]
internal partial class _page_aspx : WebApplication12.About
{{
    protected override void InitializeComponents()
    {{
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
        var generated = @$"using System;
using System.Web;

[Microsoft.AspNetCore.SystemWebAdapters.UI.AspxPageAttribute(""/page.aspx"")]
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
        var generated = @$"using System;
using System.Web;

[Microsoft.AspNetCore.SystemWebAdapters.UI.AspxPageAttribute(""/page.aspx"")]
internal partial class _page_aspx : WebApplication12.About
{{
    protected override void InitializeComponents()
    {{
        var control_1 = new global::System.Web.UI.HtmlControls.HtmlGenericControl(""div"");
        control_1.ID = ""hi"";
        hi = control_1;
        Controls.Add(control_1);
        var control_2 = new global::System.Web.UI.LiteralControl(""{NewLine}"");
        Controls.Add(control_2);
    }}
    protected global::System.Web.UI.HtmlControls.HtmlGenericControl hi;
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
    public async Task NestedTextBox()
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
        var generated = @$"using System;
using System.Web;

[Microsoft.AspNetCore.SystemWebAdapters.UI.AspxPageAttribute(""/page.aspx"")]
internal partial class _page_aspx : WebApplication12.About
{{
    protected override void InitializeComponents()
    {{
        var control_1 = new global::System.Web.UI.HtmlControls.HtmlForm();
        control_1.ID = ""frm"";
        frm = control_1;
        Controls.Add(control_1);
        {{
            var control_1_1 = new global::System.Web.UI.LiteralControl(""{NewLine}    "");
            control_1.Controls.Add(control_1_1);
            var control_1_2 = new global::System.Web.UI.WebControls.TextBox();
            control_1_2.ID = ""txt"";
            txt = control_1_2;
            control_1.Controls.Add(control_1_2);
            var control_1_3 = new global::System.Web.UI.LiteralControl(""{NewLine}"");
            control_1.Controls.Add(control_1_3);
        }}
        var control_2 = new global::System.Web.UI.LiteralControl(""{NewLine}"");
        Controls.Add(control_2);
    }}
    protected global::System.Web.UI.HtmlControls.HtmlForm frm;
    protected global::System.Web.UI.WebControls.TextBox txt;
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
    public async Task NestedLiterals()
    {
        const string BaseClass = @"namespace WebApplication12
{
    public class About : global::System.Web.UI.Page
    {
    }
}";
        var aspx = @"<%@ Page Title=""About"" Language=""C#"" MasterPageFile=""~/Site.Master"" AutoEventWireup=""true"" CodeBehind=""About.aspx.cs"" Inherits=""WebApplication12.About"" %>
<h1>Hello</h1>
";
        var generated = @$"using System;
using System.Web;

[Microsoft.AspNetCore.SystemWebAdapters.UI.AspxPageAttribute(""/page.aspx"")]
internal partial class _page_aspx : WebApplication12.About
{{
    protected override void InitializeComponents()
    {{
        var control_1 = new global::System.Web.UI.LiteralControl(""<h1>"");
        Controls.Add(control_1);
        var control_2 = new global::System.Web.UI.LiteralControl(""Hello"");
        Controls.Add(control_2);
        var control_3 = new global::System.Web.UI.LiteralControl(""</h1>"");
        Controls.Add(control_3);
        var control_4 = new global::System.Web.UI.LiteralControl(""{NewLine}"");
        Controls.Add(control_4);
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
    public async Task ScriptRunAtServer()
    {
        // Not working on non-Windows - probably due to some newline issues
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        const string BaseClass = @"namespace WebApplication12
{
    public class About : global::System.Web.UI.Page
    {
    }
}";
        var aspx = @"<%@ Page Title=""About"" Language=""C#"" MasterPageFile=""~/Site.Master"" AutoEventWireup=""true"" CodeBehind=""About.aspx.cs"" Inherits=""WebApplication12.About"" %>
<h1>Hello</h1>
<script runat=""server"">
    protected void Page_PreInit(object sender, EventArgs e)
    {
    }
</script>";
        var generated = @$"using System;
using System.Web;

[Microsoft.AspNetCore.SystemWebAdapters.UI.AspxPageAttribute(""/page.aspx"")]
internal partial class _page_aspx : WebApplication12.About
{{
    protected override void InitializeComponents()
    {{
        var control_1 = new global::System.Web.UI.LiteralControl(""<h1>"");
        Controls.Add(control_1);
        var control_2 = new global::System.Web.UI.LiteralControl(""Hello"");
        Controls.Add(control_2);
        var control_3 = new global::System.Web.UI.LiteralControl(""</h1>"");
        Controls.Add(control_3);
        var control_4 = new global::System.Web.UI.LiteralControl(""{NewLine}"");
        Controls.Add(control_4);
    }}
    #line (4, 25) - (7, 7) ""page.aspx""
    protected void Page_PreInit(object sender, EventArgs e)
    {{
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
