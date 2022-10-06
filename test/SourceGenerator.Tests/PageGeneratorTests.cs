// MIT License.

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
        var aspx = "<%@ Page Title=\"About\" Language=\"C#\" AutoEventWireup=\"true\" CodeBehind=\"About.aspx.cs\" Inherits=\"WebApplication12.About\" %>";
        var generated = @$"using System;
using System.Web;
using System.Web.UI;
[Microsoft.AspNetCore.SystemWebAdapters.UI.AspxPageAttribute(""/page.aspx"")]
internal partial class page_aspx : WebApplication12.About, global::System.Web.SessionState.IRequiresSessionState
{{
    protected override void FrameworkInitialize()
    {{
        base.FrameworkInitialize();
        Title = ""About"";
        BuildControlTree(this);
    }}
    private void BuildControlTree(Control control)
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
        var aspx = @"<%@ Page Title=""About"" Language=""C#"" AutoEventWireup=""true"" CodeBehind=""About.aspx.cs"" Inherits=""WebApplication12.About"" %>
<div />
";
        var generated = $@"using System;
using System.Web;
using System.Web.UI;
[Microsoft.AspNetCore.SystemWebAdapters.UI.AspxPageAttribute(""/page.aspx"")]
internal partial class page_aspx : WebApplication12.About, global::System.Web.SessionState.IRequiresSessionState
{{
    protected override void FrameworkInitialize()
    {{
        base.FrameworkInitialize();
        Title = ""About"";
        BuildControlTree(this);
    }}
    private void BuildControlTree(Control control)
    {{
        var control_1 = new global::System.Web.UI.LiteralControl(""<div />{NewLine}"");
        control.Controls.Add(control_1);
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
    public async Task HeadWithRunatId()
    {
        const string BaseClass = @"namespace WebApplication12
{
    public partial class About : global::System.Web.UI.Page, global::System.Web.SessionState.IRequiresSessionState
    {
    }
}";
        var aspx = @"<%@ Page Title=""About"" Language=""C#"" AutoEventWireup=""true"" CodeBehind=""About.aspx.cs"" Inherits=""WebApplication12.About"" %>
<head runat=""server"" id=""hi"" />
";
        var generated = @$"using System;
using System.Web;
using System.Web.UI;
[Microsoft.AspNetCore.SystemWebAdapters.UI.AspxPageAttribute(""/page.aspx"")]
internal partial class page_aspx : WebApplication12.About, global::System.Web.SessionState.IRequiresSessionState
{{
    protected override void FrameworkInitialize()
    {{
        base.FrameworkInitialize();
        Title = ""About"";
        BuildControlTree(this);
    }}
    private void BuildControlTree(Control control)
    {{
        var control_1 = new System.Web.UI.HtmlControls.HtmlHead();
        control_1.ID = ""hi"";
        hi = control_1;
        control.Controls.Add(control_1);
        var control_2 = new global::System.Web.UI.LiteralControl(""{NewLine}"");
        control.Controls.Add(control_2);
    }}
}}
namespace WebApplication12
{{
    partial class About
    {{
        protected System.Web.UI.HtmlControls.HtmlHead hi;
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
    public async Task NestedTextBox()
    {
        const string BaseClass = @"namespace WebApplication12
{
    public partial class About : global::System.Web.UI.Page
    {
    }
}";
        var aspx = @"<%@ Page Title=""About"" Language=""C#"" AutoEventWireup=""true"" CodeBehind=""About.aspx.cs"" Inherits=""WebApplication12.About"" %>
<form id=""frm"" runat=""server"">
    <asp:TextBox id=""txt"" runat=""server"" />
</form>
";
        var generated = @$"using System;
using System.Web;
using System.Web.UI;
[Microsoft.AspNetCore.SystemWebAdapters.UI.AspxPageAttribute(""/page.aspx"")]
internal partial class page_aspx : WebApplication12.About, global::System.Web.SessionState.IRequiresSessionState
{{
    protected override void FrameworkInitialize()
    {{
        base.FrameworkInitialize();
        Title = ""About"";
        BuildControlTree(this);
    }}
    private void BuildControlTree(Control control)
    {{
        var control_1 = new System.Web.UI.HtmlControls.HtmlForm();
        control_1.ID = ""frm"";
        frm = control_1;
        control.Controls.Add(control_1);
        {{
            var control_1_1 = new global::System.Web.UI.LiteralControl(""{NewLine}    "");
            control_1.Controls.Add(control_1_1);
            var control_1_2 = new System.Web.UI.WebControls.TextBox();
            control_1_2.ID = ""txt"";
            txt = control_1_2;
            control_1.Controls.Add(control_1_2);
            var control_1_3 = new global::System.Web.UI.LiteralControl(""{NewLine}"");
            control_1.Controls.Add(control_1_3);
        }}
        var control_2 = new global::System.Web.UI.LiteralControl(""{NewLine}"");
        control.Controls.Add(control_2);
    }}
}}
namespace WebApplication12
{{
    partial class About
    {{
        protected System.Web.UI.HtmlControls.HtmlForm frm;
        protected System.Web.UI.WebControls.TextBox txt;
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
    public async Task NestedLiterals()
    {
        const string BaseClass = @"namespace WebApplication12
{
    public class About : global::System.Web.UI.Page
    {
    }
}";
        var aspx = @"<%@ Page Title=""About"" Language=""C#"" AutoEventWireup=""true"" CodeBehind=""About.aspx.cs"" Inherits=""WebApplication12.About"" %>
<h1>Hello</h1>
";
        var generated = @$"using System;
using System.Web;
using System.Web.UI;
[Microsoft.AspNetCore.SystemWebAdapters.UI.AspxPageAttribute(""/page.aspx"")]
internal partial class page_aspx : WebApplication12.About, global::System.Web.SessionState.IRequiresSessionState
{{
    protected override void FrameworkInitialize()
    {{
        base.FrameworkInitialize();
        Title = ""About"";
        BuildControlTree(this);
    }}
    private void BuildControlTree(Control control)
    {{
        var control_1 = new global::System.Web.UI.LiteralControl(""<h1>Hello</h1>{NewLine}"");
        control.Controls.Add(control_1);
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
        var aspx = @"<%@ Page Title=""About"" Language=""C#"" AutoEventWireup=""true"" CodeBehind=""About.aspx.cs"" Inherits=""WebApplication12.About"" %>
<h1>Hello</h1>
<script runat=""server"">
    protected void Page_PreInit(object sender, EventArgs e)
    {
    }
</script>";
        var generated = @$"using System;
using System.Web;
using System.Web.UI;
[Microsoft.AspNetCore.SystemWebAdapters.UI.AspxPageAttribute(""/page.aspx"")]
internal partial class page_aspx : WebApplication12.About, global::System.Web.SessionState.IRequiresSessionState
{{
    protected override void FrameworkInitialize()
    {{
        base.FrameworkInitialize();
        Title = ""About"";
        BuildControlTree(this);
    }}
    private void BuildControlTree(Control control)
    {{
        var control_1 = new global::System.Web.UI.LiteralControl(""<h1>Hello</h1>{NewLine}"");
        control.Controls.Add(control_1);
    }}
    #line (4, 25) - (7, 7) ""page.aspx""
    protected void Page_PreInit(object sender, EventArgs e)
    {{
    }}
    #line default
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
