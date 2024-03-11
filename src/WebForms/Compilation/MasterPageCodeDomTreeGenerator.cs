// MIT License.

using System.CodeDom;
using System.Globalization;
using System.Web.UI;

namespace System.Web.Compilation;
internal class MasterPageCodeDomTreeGenerator : TemplateControlCodeDomTreeGenerator
{

    private const string _masterPropertyName = "Master";
    protected MasterPageParser _masterPageParser;

    new MasterPageParser Parser { get { return _masterPageParser; } }

    internal MasterPageCodeDomTreeGenerator(MasterPageParser parser) : base(parser)
    {
        _masterPageParser = parser;
    }

    protected override void BuildDefaultConstructor()
    {
        base.BuildDefaultConstructor();

        foreach (string placeHolderID in Parser.PlaceHolderList)
        {
            BuildAddContentPlaceHolderNames(InitMethod, placeHolderID);
        }
    }

    private static void BuildAddContentPlaceHolderNames(CodeMemberMethod method, string placeHolderID)
    {
        CodePropertyReferenceExpression propertyExpr = new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "ContentPlaceHolders");
        CodeExpressionStatement stmt = new CodeExpressionStatement();
        stmt.Expression = new CodeMethodInvokeExpression(propertyExpr, "Add", new CodePrimitiveExpression(placeHolderID.ToLower(CultureInfo.InvariantCulture)));

        method.Statements.Add(stmt);
    }

    protected override void BuildMiscClassMembers()
    {
        base.BuildMiscClassMembers();

        if (Parser.MasterPageType != null)
        {
            BuildStronglyTypedProperty(_masterPropertyName, Parser.MasterPageType);
        }
    }
}
