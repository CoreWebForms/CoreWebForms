// MIT License.

namespace System.Web.UI;
public interface IExpressionsAccessor
{

    bool HasExpressions
    {
        get;
    }

    ExpressionBindingCollection Expressions
    {
        get;
    }
}
