using Microsoft.AspNetCore.SystemWebAdapters.UI.Generator;
using Xunit;

using VerifyCS = Microsoft.AspNetCore.SystemWebAdapters.UI.Generator.Tests.GeneratorVerifier<
    Microsoft.AspNetCore.SystemWebAdapters.UI.Generator.PageGenerator>;

namespace SystemWebAdapters.UI.Generator.Tests;

public class PageGeneratorTests
{
    [Fact]
    public async Task Empty()
    {
        await new VerifyCS.Test().RunAsync();
    }

    [Fact]
    public async Task EmptyAspx()
    {
        var aspx = "<%@ Page Title=\"About\" Language=\"C#\" MasterPageFile=\"~/Site.Master\" AutoEventWireup=\"true\" CodeBehind=\"About.aspx.cs\" Inherits=\"WebApplication12.About\" %>\r\n";
        var generated = @"public partial class About : global::System.Web.UI.Page
{
}
";

        await new VerifyCS.Test()
        {
            TestState =
            {
                AdditionalFiles =
                {
                    ("/page.aspx", aspx),
                },
                GeneratedSources =
                {
                    (typeof(PageGenerator), "page.aspx.g.cs", generated),
                },
            },
        }.RunAsync();
    }
}