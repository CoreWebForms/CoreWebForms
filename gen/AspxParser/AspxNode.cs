// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.AspNetCore.SystemWebAdapters.UI.PageParser.ParserImpl;

namespace Microsoft.AspNetCore.SystemWebAdapters.UI.PageParser;

public abstract class AspxNode
{
    public AspxNode Parent { get; private set; }

    public List<AspxNode> Children { get; private set; }

    protected AspxNode()
    {
        Children = new List<AspxNode>();
    }

    public void AddChild(AspxNode node)
    {
        Debug.Assert(node.Parent == null);

        Children.Add(node);
        node.Parent = this;
    }

    public abstract T Accept<T>(IAspxVisitor<T> visitor);

    public sealed override string ToString()
    {
        var writer = new StringWriter();
        Accept(new AspxOutputVisitor(writer));
        return writer.ToString();
    }

    public sealed class Root : AspxNode
    {
        public override T Accept<T>(IAspxVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public abstract class Located : AspxNode
    {
        public Location Location { get; }

        protected Located(Location location)
        {
            Location = location;
        }
    }

    public abstract class HtmlTag : Located
    {
        public string Name { get; }

        public TagAttributes Attributes { get; }

        protected HtmlTag(string name, TagAttributes attributes, Location location)
            : base(location)
        {
            Name = name;
            Attributes = attributes;
        }
    }

    public sealed class OpenHtmlTag : HtmlTag
    {
        public OpenHtmlTag(string name, TagAttributes attributes, Location location)
            : base(name, attributes, location)
        {
        }

        public override T Accept<T>(IAspxVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public sealed class SelfClosingHtmlTag : HtmlTag
    {
        public SelfClosingHtmlTag(string name, TagAttributes attributes, Location location)
            : base(name, attributes, location)
        {
        }

        public override T Accept<T>(IAspxVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public abstract class AspxTag : Located
    {
        public string Prefix { get; }

        public string ControlName { get; }

        public TagAttributes Attributes { get; }

        protected AspxTag(string prefix, string controlName, TagAttributes attributes, Location location)
            : base(location)
        {
            Prefix = prefix;
            ControlName = controlName;
            Attributes = attributes;
        }
    }

    public sealed class OpenAspxTag : AspxTag
    {
        public OpenAspxTag(string prefix, string controlName, TagAttributes attributes, Location location)
            : base(prefix, controlName, attributes, location)
        {
        }

        public override T Accept<T>(IAspxVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public sealed class SelfClosingAspxTag : AspxTag
    {
        public SelfClosingAspxTag(string prefix, string controlName, TagAttributes attributes, Location location)
            : base(prefix, controlName, attributes, location)
        {
        }

        public override T Accept<T>(IAspxVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public abstract class CloseTag : Located
    {
        protected CloseTag(Location location)
            : base(location)
        {
        }
    }

    public sealed class CloseAspxTag : CloseTag
    {
        public string ControlName { get; }

        public string Prefix { get; }

        public CloseAspxTag(string prefix, string name, Location location)
            : base(location)
        {
            ControlName = name;
            Prefix = prefix;
        }

        public override T Accept<T>(IAspxVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public sealed class CloseHtmlTag : CloseTag
    {
        public string Name { get; }

        public CloseHtmlTag(string name, Location location)
            : base(location)
        {
            Name = name;
        }

        public override T Accept<T>(IAspxVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public sealed class AspxDirective : Located
    {
        public string Name { get; }

        public TagAttributes Attributes { get; }

        public AspxDirective(string name, TagAttributes attributes, Location location)
            : base(location)
        {
            Name = name;
            Attributes = attributes;
        }

        public override T Accept<T>(IAspxVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public abstract class AspxExpressionTag : Located
    {
        public string Expression { get; set; }

        protected AspxExpressionTag(string expression, Location location)
            : base(location)
        {
            Expression = expression;
        }
    }

    public sealed class DataBinding : AspxExpressionTag
    {
        public DataBinding(string code, Location location)
            : base(code, location)
        {
        }

        public override T Accept<T>(IAspxVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public sealed class CodeRender : AspxExpressionTag
    {
        public CodeRender(string code, Location location)
            : base(code, location)
        {
        }

        public override T Accept<T>(IAspxVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public sealed class CodeRenderExpression : AspxExpressionTag
    {
        public CodeRenderExpression(string code, Location location)
            : base(code, location)
        {
        }

        public override T Accept<T>(IAspxVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public sealed class CodeRenderEncode : AspxExpressionTag
    {
        public CodeRenderEncode(string code, Location location)
            : base(code, location)
        {
        }

        public override T Accept<T>(IAspxVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public sealed class Literal : Located
    {
        public string Text { get; }

        public Literal(string text, Location location)
            : base(location)
        {
            Text = text;
        }

        public override T Accept<T>(IAspxVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
