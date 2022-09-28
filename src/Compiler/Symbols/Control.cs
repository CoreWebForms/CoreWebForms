// MIT License.

#nullable enable

using System;
using System.Collections.Immutable;

namespace Microsoft.AspNetCore.SystemWebAdapters.Compiler.Symbols;

internal readonly record struct Position(int Line, int Column);

internal readonly record struct Location(string Path, Position Start, Position End)
{
    public static Location operator +(Location left, Location right)
    {
        if (!string.Equals(left.Path, right.Path, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Location must be from same file to combine");
        }

        return left with { End = right.End, };
    }
}

public record class ParsedPage
{
    private ImmutableArray<string> _additionalFiles;
    private ImmutableArray<string> _contentPlaceholders;
    private ImmutableArray<Script> _scripts;
    private ImmutableArray<Template> _templates;
    private ImmutableArray<string> _errors;

    public PagePath File { get; init; }

    public PagePath? MasterPage => Directive.MasterPageFile is { } file ? new(file) : default;

    internal Syntax.DirectiveDetails Directive { get; init; }

    internal Control? Root { get; init; }

    public ImmutableArray<string> Errors
    {
        get => Control.EnsureInitialized(_errors);
        init => _errors = value;
    }

    internal ImmutableArray<Template> Templates
    {
        get => Control.EnsureInitialized(_templates);
        init => _templates = value;
    }

    internal ImmutableArray<Script> Scripts
    {
        get => Control.EnsureInitialized(_scripts);
        init => _scripts = value;
    }

    internal ImmutableArray<string> ContentPlaceholders
    {
        get => Control.EnsureInitialized(_contentPlaceholders);
        init => _contentPlaceholders = value;
    }

    public ImmutableArray<string> AdditionalFiles
    {
        get => Control.EnsureInitialized(_additionalFiles);
        set => _additionalFiles = value;
    }
}

internal abstract record class Control
{
    private readonly ImmutableArray<Control> _children;

    public string? Id { get; init; }

    public ImmutableArray<Control> Children
    {
        get => _children.IsDefault ? ImmutableArray<Control>.Empty : _children;
        init => _children = value;
    }

    internal static ImmutableArray<T> EnsureInitialized<T>(ImmutableArray<T> array) => array.IsDefault ? ImmutableArray<T>.Empty : array;
}

internal record LiteralControl(string Text, Location Location) : TypedControl(new QName("System.Web.UI", "LiteralControl"), Location)
{
    public LiteralControl Combine(LiteralControl next)
        => this with
        {
            Text = Text + next.Text,
            Location = Location,
        };
}

internal record TypedControl(QName Type, Location Location) : Control
{
    private ImmutableArray<Attribute> _attributes;
    private ImmutableArray<TemplateProperty> _templates;

    public ImmutableArray<Attribute> Attributes
    {
        get => EnsureInitialized(_attributes);
        init => _attributes = value;
    }

    public ImmutableArray<TemplateProperty> Templates
    {
        get => EnsureInitialized(_templates);
        init => _templates = value;
    }
}

internal record Attribute(string Key, string Value, DataType Kind);

internal record CodeControl(string Expression, Location location) : Control;

internal record Root : Control
{
}

internal record Property(string Name) : Control
{
}

internal record TemplateProperty(string Name, Control Control)
{
}

internal record Template(string Id, string PlaceholderId, ImmutableArray<Control> Controls)
{
}

internal readonly record struct Script(ImmutableArray<TextLine> Lines);

internal readonly record struct TextLine(string Text, Location Location);

