// MIT License.

namespace System.Web.Compilation;

using System;
using System.CodeDom;
using System.ComponentModel.Design;
using System.Web;
#if !FEATURE_PAL
#endif // !FEATURE_PAL
using System.Web.UI;

public abstract class ExpressionBuilder
{
    private static ExpressionBuilder dataBindingExpressionBuilder;

    internal virtual void BuildExpression(BoundPropertyEntry bpe, ControlBuilder controlBuilder,
        CodeExpression controlReference, CodeStatementCollection methodStatements, CodeStatementCollection statements, CodeLinePragma linePragma, ref bool hasTempObject)
    {

        CodeExpression codeExpression = GetCodeExpression(bpe,
            bpe.ParsedExpressionData, new ExpressionBuilderContext(controlBuilder.VirtualPath));

        CodeDomUtility.CreatePropertySetStatements(methodStatements, statements,
            controlReference, bpe.Name, bpe.Type,
            codeExpression,
            linePragma);
    }

    internal static ExpressionBuilder GetExpressionBuilder(string expressionPrefix, VirtualPath virtualPath)
    {
        return GetExpressionBuilder(expressionPrefix, virtualPath, null);
    }

    internal static ExpressionBuilder GetExpressionBuilder(string expressionPrefix, VirtualPath virtualPath, IDesignerHost host)
    {
        // If there is no expressionPrefix, it's a v1 style databinding expression
        if (expressionPrefix.Length == 0)
        {
            if (dataBindingExpressionBuilder == null)
            {
                dataBindingExpressionBuilder = new DataBindingExpressionBuilder();
            }
            return dataBindingExpressionBuilder;
        }

#if PORT_CONFIG
        CompilationSection config = null;

        // If we are in the designer, we need to access IWebApplication config instead
#if !FEATURE_PAL // FEATURE_PAL does not support designer-based features
        if (host != null)
        {
            IWebApplication webapp = (IWebApplication)host.GetService(typeof(IWebApplication));
            if (webapp != null)
            {
                config = webapp.OpenWebConfiguration(true).GetSection("system.web/compilation") as CompilationSection;
            }
        }
#endif // !FEATURE_PAL

        // If we failed to get config from the designer, fall back on runtime config always
        if (config == null)
        {
            config = MTConfigUtil.GetCompilationConfig(virtualPath);
        }

        System.Web.Configuration.ExpressionBuilder builder = config.ExpressionBuilders[expressionPrefix];
        if (builder == null)
        {
            throw new HttpParseException(SR.GetString(SR.InvalidExpressionPrefix, expressionPrefix));
        }

        Type expressionBuilderType = null;
        if (host != null)
        {
            // If we are in the designer, we have to use the type resolution service
            ITypeResolutionService ts = (ITypeResolutionService)host.GetService(typeof(ITypeResolutionService));
            if (ts != null)
            {
                expressionBuilderType = ts.GetType(builder.Type);
            }
        }
        if (expressionBuilderType == null)
        {
            expressionBuilderType = builder.TypeInternal;
        }
        Debug.Assert(expressionBuilderType != null, "expressionBuilderType should not be null");

        if (!typeof(ExpressionBuilder).IsAssignableFrom(expressionBuilderType))
        {
            throw new HttpParseException(SR.GetString(SR.ExpressionBuilder_InvalidType, expressionBuilderType.FullName));
        }
        ExpressionBuilder expressionBuilder = (ExpressionBuilder)HttpRuntime.FastCreatePublicInstance(expressionBuilderType);

        return expressionBuilder;
#else
        throw new NotImplementedException("GetExpressionBuilder");
#endif
    }

    //
    // Public API
    //

    public virtual bool SupportsEvaluate
    {
        get
        {
            return false;
        }
    }

    public virtual object ParseExpression(string expression, Type propertyType, ExpressionBuilderContext context)
    {
        return null;
    }

    public abstract CodeExpression GetCodeExpression(BoundPropertyEntry entry,
        object parsedData, ExpressionBuilderContext context);

    public virtual object EvaluateExpression(object target, BoundPropertyEntry entry,
        object parsedData, ExpressionBuilderContext context)
    {
        return null;
    }

}
