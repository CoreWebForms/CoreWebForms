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
        var control0 = new global::System.Web.UI.LiteralControl(""<div />"");
        Controls.Add(control0);
        var control1 = new global::System.Web.UI.LiteralControl(""{NewLine}"");
        Controls.Add(control1);
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
        var control0 = new global::System.Web.UI.HtmlControls.HtmlGenericControl(""div"");
        control0.Id = ""hi"";
        Controls.Add(control0);
        var control1 = new global::System.Web.UI.LiteralControl(""{NewLine}"");
        Controls.Add(control1);
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
