// MIT License.

using System.Collections;
using System.Text.RegularExpressions;

namespace System.Web.UI;

// TODO: COPY
public abstract class ControlBuilder
{
    public void SetControlType(Type type) { }

    public string TagName { get; set; }

    public ControlBuilder ParentBuilder { get; private set; }

    public virtual void Init(TemplateParser parser, ControlBuilder parentBuilder,
                                Type type, string tagName, string ID, IDictionary attribs)
    {
    }

    public virtual void AppendSubBuilder(ControlBuilder subBuilder) { }

    /// <internalonly/>
    /// <devdoc>
    /// </devdoc>
    public virtual void AppendLiteralString(string s) { }

    internal static Regex expressionBuilderRegex;

    public object Parser { get; internal set; }
    public string ID { get; internal set; }
    public int Line { get; internal set; }

    public virtual bool HasBody() => true;

    public virtual Type GetChildControlType(string tagName, IDictionary attribs) => null;

    public virtual bool AllowWhitespaceLiterals() => true;

    internal VirtualPath VirtualPath { get; set; }

    public virtual object BuildObject()
    {
        return null;
    }

    internal ControlBuilder CreateChildBuilder(string filter, string realTagName, object attribs, object templateParser, ControlBuilder parentBuilder, string id, int lineNumber, VirtualPath currentVirtualPath, ref Type childType, bool v)
    {
        throw new NotImplementedException();
    }

    internal void PreprocessAttribute(string empty, string v1, string id, bool v2)
    {
        throw new NotImplementedException();
    }

    internal void CloseControl()
    {
        throw new NotImplementedException();
    }

    internal static ControlBuilder CreateBuilderFromType(object templateParser, ControlBuilder parentBuilder, Type type, object value1, object value2, IDictionary attributes, int lineNumber, string virtualPathString)
    {
        throw new NotImplementedException();
    }

    internal bool NeedsTagInnerText()
    {
        throw new NotImplementedException();
    }

    internal void SetTagInnerText(string v)
    {
        throw new NotImplementedException();
    }

    internal bool HtmlDecodeLiterals()
    {
        throw new NotImplementedException();
    }
}
