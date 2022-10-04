// MIT License.

using Microsoft.AspNetCore.SystemWebAdapters.Compiler;
using Microsoft.AspNetCore.SystemWebAdapters.Compiler.Symbols;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.UI.Compiler.Tests;

public class PageSymbolTests
{
    private readonly string Path = Guid.NewGuid().ToString();

    [Fact]
    public void HeadWithRunat()
    {
        // Arrange
        const string PageContents = @"<%@ Page Title=""About"" Language=""C#"" AutoEventWireup=""true"" CodeBehind=""About.aspx.cs"" Inherits=""WebApplication12.About"" %>
<head runat=""server"" id=""hi"" />";

        // Act
        var result = SymbolCreator.ParsePage(Path, PageContents, GetControls());

        // Assert
        Assert.Empty(result.Templates);
        Assert.Empty(result.Scripts);
        Assert.Empty(result.Errors);

        Assert.True(result.Root is Root
        {
            Children:
            [
                TypedControl { Id: "hi", Children: [], Type: { Name: "HtmlHead", Namespace: "System.Web.UI.HtmlControls" } }
            ]
        });
    }

    [Fact]
    public void HeadWithNoRunat()
    {
        // Arrange
        const string PageContents = @"<%@ Page Title=""About"" Language=""C#"" AutoEventWireup=""true"" CodeBehind=""About.aspx.cs"" Inherits=""WebApplication12.About"" %>
<head id=""hi"" />";

        // Act
        var result = SymbolCreator.ParsePage(Path, PageContents, GetControls());

        // Assert
        Assert.Empty(result.Templates);
        Assert.Empty(result.Scripts);
        Assert.Empty(result.Errors);

        Assert.True(result.Root is Root
        {
            Children:
            [
                LiteralControl { Text: @"<head id=""hi"" />" }
            ]
        });
    }

    [Fact]
    public void SimpleAspxSelfclosing()
    {
        // Arrange
        const string PageContents = @"<%@ Page Title=""About"" Language=""C#"" AutoEventWireup=""true"" CodeBehind=""About.aspx.cs"" Inherits=""WebApplication12.About"" %>
<aspx:TextBox runat=""server"" id=""hi"" />";

        // Act
        var result = SymbolCreator.ParsePage(Path, PageContents, GetControls());

        // Assert
        Assert.Empty(result.Templates);
        Assert.Empty(result.Scripts);
        Assert.Empty(result.Errors);

        Assert.True(result.Root is Root
        {
            Children:
            [
                TypedControl { Id: "hi", Children: [], Type: { Name: "TextBox", Namespace: "System.Web.UI.WebControls" } }
            ]
        });
    }

    [Fact]
    public void SimpleAspx()
    {
        // Arrange
        const string PageContents = @"<%@ Page Title=""About"" Language=""C#"" AutoEventWireup=""true"" CodeBehind=""About.aspx.cs"" Inherits=""WebApplication12.About"" %>
<aspx:TextBox runat=""server"" id=""hi""></aspx:TextBox>";

        // Act
        var result = SymbolCreator.ParsePage(Path, PageContents, GetControls());

        // Assert
        Assert.Empty(result.Templates);
        Assert.Empty(result.Scripts);
        Assert.Empty(result.Errors);

        Assert.True(result.Root is Root
        {
            Children:
            [
                TypedControl { Id: "hi", Children: [], Type: { Name: "TextBox", Namespace: "System.Web.UI.WebControls" } }
            ]
        });
    }

    [Fact]
    public void SimpleAspxWithAdditionalAttribute()
    {
        // Arrange
        const string PageContents = @"<%@ Page Title=""About"" Language=""C#"" AutoEventWireup=""true"" CodeBehind=""About.aspx.cs"" Inherits=""WebApplication12.About"" %>
<aspx:TextBox runat=""server"" id=""hi"" a=""2""></aspx:TextBox>";

        // Act
        var result = SymbolCreator.ParsePage(Path, PageContents, GetControls());

        // Assert
        Assert.Empty(result.Templates);
        Assert.Empty(result.Scripts);
        Assert.Empty(result.Errors);

        Assert.True(result.Root is Root
        {
            Children:
            [
                TypedControl
            {
                Id: "hi",
                Children: [],
                Type: { Name: "TextBox", Namespace: "System.Web.UI.WebControls" },
                Attributes: [{ Key: "a", Value: "2" }]
            }
            ]
        });
    }

    [Fact]
    public void Content()
    {
        // Arrange
        const string PageContents = @"<%@ Page Title=""About"" Language=""C#"" AutoEventWireup=""true"" CodeBehind=""About.aspx.cs"" Inherits=""WebApplication12.About"" %>
<asp:Content ID=""BodyContent"" ContentPlaceHolderID=""MainContent"" runat=""server"" />";

        // Act
        var result = SymbolCreator.ParsePage(Path, PageContents, GetControls());

        // Assert
        Assert.Empty(result.Scripts);
        Assert.Empty(result.Errors);
        Assert.True(result.Root is Root { Children: [] });

        var template = Assert.Single(result.Templates);

        Assert.Equal("BodyContent", template.Id);
        Assert.Equal("MainContent", template.PlaceholderId);
        Assert.Empty(template.Controls);
    }

    [Fact]
    public void NestedLiteralsAreFlattened()
    {
        // Arrange
        const string PageContents = @"<%@ Page Title=""About"" Language=""C#"" AutoEventWireup=""true"" CodeBehind=""About.aspx.cs"" Inherits=""WebApplication12.About"" %>
Button clicked: <b><asp:TextBox runat=""server"" /></b>";

        // Act
        var result = SymbolCreator.ParsePage(Path, PageContents, GetControls());

        // Assert
        Assert.Empty(result.Templates);
        Assert.Empty(result.Scripts);
        Assert.Empty(result.Errors);

        if (result.Root is Root { Children: [LiteralControl { Text: { } text }, TypedControl, LiteralControl { Text: "</b>" }] })
        {
            Assert.Equal($"{Environment.NewLine}Button clicked: <b>", text);
        }
        else
        {
            Assert.True(false);
        }
    }

    private static IControlLookup GetControls()
        => new Controls
        {
            new ControlInfo("System.Web.UI.WebControls", "TextBox"),
        };

    private sealed class Controls : Dictionary<string, ControlInfo>, IControlLookup
    {
        public void Add(ControlInfo info)
            => Add(info.Name, info);

        bool IControlLookup.TryGetControl(string prefix, string name, out ControlInfo info)
            => TryGetValue(name, out info!);
    }
}
