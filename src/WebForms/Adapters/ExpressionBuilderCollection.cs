// MIT License.

using System.CodeDom;
using System.Web.Compilation;
using System.Web.UI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using static WebForms.ExpressionBuilderCollection;

namespace WebForms;

internal class ExpressionBuilderCollection(ILogger<ExpressionBuilderCollection> logger, IOptions<ExpressionOption> options)
{
    public class ExpressionOption : Dictionary<string, Func<ExpressionBuilder>>
    {
    }

    public ExpressionBuilder GetBuilder(string prefix)
    {
        if (options.Value.TryGetValue(prefix, out var builder))
        {
            return builder();
        }

        logger.LogError("Unknown prefix '{Prefix}' for expression builder", prefix);

        return new DefaultExpressionBuilder();
    }

    private sealed class DefaultExpressionBuilder : ExpressionBuilder
    {
        public override CodeExpression GetCodeExpression(BoundPropertyEntry entry, object parsedData, ExpressionBuilderContext context)
            => new();
    }
}
