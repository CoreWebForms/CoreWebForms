// MIT License.

using System.CodeDom;
using System.Web.Routing;
using System.Web.UI;

namespace System.Web.Compilation;

public class RouteUrlExpressionBuilder : ExpressionBuilder
{
    public override bool SupportsEvaluate
    {
        get
        {
            return true;
        }
    }

    public override CodeExpression GetCodeExpression(BoundPropertyEntry entry,
        object parsedData, ExpressionBuilderContext context)
    {

        return new CodeMethodInvokeExpression(
            new CodeTypeReferenceExpression(this.GetType()),
            "GetRouteUrl",
            new CodeThisReferenceExpression(),
            new CodePrimitiveExpression(entry.Expression.Trim()));
    }

    public override object EvaluateExpression(object target, BoundPropertyEntry entry,
        object parsedData, ExpressionBuilderContext context)
    {

        return GetRouteUrl(context.TemplateControl, entry.Expression.Trim());
    }

    public static bool TryParseRouteExpression(string expression, RouteValueDictionary routeValues, out string routeName)
    {
        routeName = null;

        if (string.IsNullOrEmpty(expression))
        {
            return false;
        }

        var pieces = expression.Split([',']);
        foreach (var piece in pieces)
        {
            var subs = piece.Split(['=']);
            // Make sure we have exactly <key> = <value>
            if (subs.Length != 2)
            {
                return false;
            }

            var key = subs[0].Trim();
            var value = subs[1].Trim();

            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            if (key.Equals("RouteName", StringComparison.OrdinalIgnoreCase))
            {
                routeName = value;
            }
            else
            {
                routeValues[key] = value;
            }
        }

        return true;
    }

    // Format will be <%$ ExpPrefix: RouteName = <name>, Key=Value, Key2=Value2 %>
    public static string GetRouteUrl(Control control, string expression)
    {
        if (control == null)
        {
            throw new ArgumentNullException(nameof(control));
        }

        var routeParams = new RouteValueDictionary();

        if (TryParseRouteExpression(expression, routeParams, out var routeName))
        {
            return control.GetRouteUrl(routeName, routeParams);
        }
        else
        {
            throw new InvalidOperationException(SR.GetString(SR.RouteUrlExpression_InvalidExpression));
        }
    }
}
