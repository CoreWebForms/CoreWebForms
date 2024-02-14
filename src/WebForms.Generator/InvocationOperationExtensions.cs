// MIT License.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace WebForms.Generator;

internal record InterceptorLocation(string FilePath, int Line, int Character);

internal static class InvocationOperationExtensions
{
    public static InterceptorLocation GetLocation(this IInvocationOperation operation)
    {
        // The invocation expression consists of two properties:
        // - Expression: which is a `MemberAccessExpressionSyntax` that represents the method being invoked.
        // - ArgumentList: the list of arguments being invoked.
        // Here, we resolve the `MemberAccessExpressionSyntax` to get the location of the method being invoked.
        var memberAccessorExpression = ((MemberAccessExpressionSyntax)((InvocationExpressionSyntax)operation.Syntax).Expression);

        // The `MemberAccessExpressionSyntax` in turn includes three properties:
        // - Expression: the expression that is being accessed.
        // - OperatorToken: the operator token, typically the dot separate.
        // - Name: the name of the member being accessed, typically `MapGet` or `MapPost`, etc.
        // Here, we resolve the `Name` to extract the location of the method being invoked.
        var invocationNameSpan = memberAccessorExpression.Name.Span;

        // Resolve LineSpan associated with the name span so we can resolve the line and character number.
        var lineSpan = operation.Syntax.SyntaxTree.GetLineSpan(invocationNameSpan);

        // Resolve the filepath of the invocation while accounting for source mapped paths.
        var filePath = operation.Syntax.SyntaxTree.GetInterceptorFilePath(operation.SemanticModel?.Compilation.Options.SourceReferenceResolver);

        // LineSpan.LinePosition is 0-indexed, but we want to display 1-indexed line and character numbers in the interceptor attribute.
        return new InterceptorLocation(filePath, lineSpan.StartLinePosition.Line + 1, lineSpan.StartLinePosition.Character + 1);
    }

    public static bool TryGetMapMethodName(this SyntaxNode node, out string? methodName)
    {
        methodName = default;
        // Given an invocation like app.MapGet, app.Map, app.MapFallback, etc. get
        // the value of the Map method being access on the the WebApplication `app`.
        if (node is InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax { Name: { Identifier: { ValueText: var method } } } })
        {
            methodName = method;
            return true;
        }
        return false;
    }

    private static IMethodSymbol? ResolveMethodFromOperation(IOperation operation, SemanticModel semanticModel) => operation switch
    {
        IArgumentOperation argument => ResolveMethodFromOperation(argument.Value, semanticModel),
        IConversionOperation conv => ResolveMethodFromOperation(conv.Operand, semanticModel),
        IDelegateCreationOperation del => ResolveMethodFromOperation(del.Target, semanticModel),
        IFieldReferenceOperation { Field.IsReadOnly: true } f when ResolveDeclarationOperation(f.Field, semanticModel) is IOperation op =>
            ResolveMethodFromOperation(op, semanticModel),
        IAnonymousFunctionOperation anon => anon.Symbol,
        ILocalFunctionOperation local => local.Symbol,
        IMethodReferenceOperation method => method.Method,
        IParenthesizedOperation parenthesized => ResolveMethodFromOperation(parenthesized.Operand, semanticModel),
        _ => null
    };

    private static IOperation? ResolveDeclarationOperation(ISymbol symbol, SemanticModel? semanticModel)
    {
        foreach (var syntaxReference in symbol.DeclaringSyntaxReferences)
        {
            var syn = syntaxReference.GetSyntax();

            if (syn is VariableDeclaratorSyntax
                {
                    Initializer:
                    {
                        Value: var expr
                    }
                })
            {
                // Use the correct semantic model based on the syntax tree
                var targetSemanticModel = semanticModel?.Compilation.GetSemanticModel(expr.SyntaxTree);
                var operation = targetSemanticModel?.GetOperation(expr);

                if (operation is not null)
                {
                    return operation;
                }
            }
        }

        return null;
    }
}
