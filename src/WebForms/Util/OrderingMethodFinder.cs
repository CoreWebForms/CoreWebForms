// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq.Expressions;

namespace System.Web.Util;

internal sealed class OrderingMethodFinder : ExpressionVisitor
{

    private bool isTopLevelMethodCall = true;

    private bool OrderingMethodFound
    {
        get;
        set;
    }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        if (isTopLevelMethodCall && QueryableUtility.IsOrderingMethod(node))
        {
            OrderingMethodFound = true;
        }

        isTopLevelMethodCall = false;
        Expression result = base.VisitMethodCall(node);
        isTopLevelMethodCall = true;
        return result;
    }

    internal static bool OrderMethodExists(Expression expression)
    {
        OrderingMethodFinder obj = new OrderingMethodFinder();
        obj.OrderingMethodFound = false;
        obj.Visit(expression);
        return obj.OrderingMethodFound;
    }
}
