// MIT License.

using System.Web.Compilation;

namespace WebForms;

internal class ExpressionBuilderOption
{
    public Func<ExpressionBuilder> Factory { get; set; }
}
