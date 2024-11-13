// MIT License.

using System.CodeDom;
using System.Web.UI;

namespace System.Web.Compilation;
internal class UserControlCodeDomTreeGenerator : TemplateControlCodeDomTreeGenerator
{

    protected UserControlParser _ucParser;

    new UserControlParser Parser { get { return _ucParser; } }

    internal UserControlCodeDomTreeGenerator(UserControlParser ucParser) : base(ucParser)
    {
        _ucParser = ucParser;
    }

    /*
     * Add metadata attributes to the class
     */
    protected override void GenerateClassAttributes()
    {

        base.GenerateClassAttributes();

        // If the user control has an OutputCache directive, generate
        // an attribute with the information about it.
        if (_sourceDataClass != null && Parser.OutputCacheParameters != null)
        {
            OutputCacheParameters cacheSettings = Parser.OutputCacheParameters;
            if (cacheSettings.Duration > 0)
            {
                CodeAttributeDeclaration attribDecl = new CodeAttributeDeclaration(
                    "System.Web.UI.PartialCachingAttribute");
                CodeAttributeArgument attribArg = new CodeAttributeArgument(
                    new CodePrimitiveExpression(cacheSettings.Duration));
                attribDecl.Arguments.Add(attribArg);
                attribArg = new CodeAttributeArgument(new CodePrimitiveExpression(cacheSettings.VaryByParam));
                attribDecl.Arguments.Add(attribArg);
                attribArg = new CodeAttributeArgument(new CodePrimitiveExpression(cacheSettings.VaryByControl));
                attribDecl.Arguments.Add(attribArg);
                attribArg = new CodeAttributeArgument(new CodePrimitiveExpression(cacheSettings.VaryByCustom));
                attribDecl.Arguments.Add(attribArg);
                attribArg = new CodeAttributeArgument(new CodePrimitiveExpression(cacheSettings.SqlDependency));
                attribDecl.Arguments.Add(attribArg);
                // TODO: Migration
                // attribArg = new CodeAttributeArgument(new CodePrimitiveExpression(UserControlParser.FSharedPartialCaching));
                attribDecl.Arguments.Add(attribArg);
                // Use the providerName argument only when targeting 4.0 and above.
                if (MultiTargetingUtil.IsTargetFramework40OrAbove)
                {
                    // TODO: Migration
                    // attribArg = new CodeAttributeArgument("ProviderName", new CodePrimitiveExpression(UserControlParser.Provider));
                    attribDecl.Arguments.Add(attribArg);
                }

                _sourceDataClass.CustomAttributes.Add(attribDecl);
            }
        }
    }
}

