// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNetCore.SystemWebAdapters.Compiler.Syntax;

/// <summary>
/// LINQ to tree extensions
/// </summary>
public static class AspxNodeExtensions
{
    public static IEnumerable<AspxNode> Descendants(this AspxNode item)
    {
        foreach (var child in item.Children)
        {
            yield return child;

            foreach (var grandChild in child.Descendants())
            {
                yield return grandChild;
            }
        }
    }

    public static IEnumerable<AspxNode> DescendantsAndSelf(this AspxNode item)
    {
        yield return item;

        foreach (var child in item.Descendants())
        {
            yield return child;
        }
    }

    public static IEnumerable<AspxNode> Ancestors(this AspxNode item)
    {
        var parent = item.Parent;
        while (parent != null)
        {
            yield return parent;
            parent = parent.Parent;
        }
    }

    public static IEnumerable<AspxNode> AncestorsAndSelf(this AspxNode item)
    {
        yield return item;

        foreach (var ancestor in item.Ancestors())
        {
            yield return ancestor;
        }
    }

    public static IEnumerable<AspxNode> Elements(this AspxNode item)
    {
        return item.Children;
    }

    public static IEnumerable<AspxNode> ElementsBeforeSelf(this AspxNode item)
    {
        if (item.Ancestors().FirstOrDefault() == null)
        {
            yield break;
        }

        foreach (var child in item.Ancestors().First().Elements())
        {
            if (item.Equals(child))
            {
                break;
            }

            yield return child;
        }
    }

    public static IEnumerable<AspxNode> ElementsAfterSelf(this AspxNode item)
    {
        if (item.Ancestors().FirstOrDefault() == null)
        {
            yield break;
        }

        var afterSelf = false;
        foreach (var child in item.Ancestors().First().Elements())
        {
            if (afterSelf)
            {
                yield return child;
            }

            if (item.Equals(child))
            {
                afterSelf = true;
            }
        }
    }

    public static IEnumerable<AspxNode> ElementsAndSelf(this AspxNode item)
    {
        yield return item;

        foreach (var child in item.Elements())
        {
            yield return child;
        }
    }

    public static IEnumerable<T> Descendants<T>(this AspxNode item)
    {
        return item.Descendants().Where(i => i is T).Cast<T>();
    }

    public static IEnumerable<T> ElementsBeforeSelf<T>(this AspxNode item)
    {
        return item.ElementsBeforeSelf().Where(i => i is T).Cast<T>();
    }

    public static IEnumerable<T> ElementsAfterSelf<T>(this AspxNode item)
    {
        return item.ElementsAfterSelf().Where(i => i is T).Cast<T>();
    }

    public static IEnumerable<T> DescendantsAndSelf<T>(this AspxNode item)
    {
        return item.DescendantsAndSelf().Where(i => i is T).Cast<T>();
    }

    public static IEnumerable<T> Ancestors<T>(this AspxNode item)
    {
        return item.Ancestors().Where(i => i is T).Cast<T>();
    }

    public static IEnumerable<T> AncestorsAndSelf<T>(this AspxNode item)
    {
        return item.AncestorsAndSelf().Where(i => i is T).Cast<T>();
    }

    public static IEnumerable<T> Elements<T>(this AspxNode item)
    {
        return item.Elements().Where(i => i is T).Cast<T>();
    }

    public static IEnumerable<T> ElementsAndSelf<T>(this AspxNode item)
    {
        return item.ElementsAndSelf().Where(i => i is T).Cast<T>();
    }

    private static IEnumerable<AspxNode> DrillDown(this IEnumerable<AspxNode> items,
        Func<AspxNode, IEnumerable<AspxNode>> function)
    {
        return items.SelectMany(function);
    }

    public static IEnumerable<T> DrillDown<T>(this IEnumerable<AspxNode> items,
        Func<AspxNode, IEnumerable<AspxNode>> function)
        where T : AspxNode
    {
        return items.SelectMany(item => function(item).OfType<T>());
    }

    public static IEnumerable<AspxNode> Descendants(this IEnumerable<AspxNode> items)
    {
        return items.DrillDown(i => i.Descendants());
    }

    public static IEnumerable<AspxNode> DescendantsAndSelf(this IEnumerable<AspxNode> items)
    {
        return items.DrillDown(i => i.DescendantsAndSelf());
    }

    public static IEnumerable<AspxNode> Ancestors(this IEnumerable<AspxNode> items)
    {
        return items.DrillDown(i => i.Ancestors());
    }

    public static IEnumerable<AspxNode> AncestorsAndSelf(this IEnumerable<AspxNode> items)
    {
        return items.DrillDown(i => i.AncestorsAndSelf());
    }

    public static IEnumerable<AspxNode> Elements(this IEnumerable<AspxNode> items)
    {
        return items.DrillDown(i => i.Elements());
    }

    public static IEnumerable<AspxNode> ElementsAndSelf(this IEnumerable<AspxNode> items)
    {
        return items.DrillDown(i => i.ElementsAndSelf());
    }

    public static IEnumerable<T> Descendants<T>(this IEnumerable<AspxNode> items)
        where T : AspxNode
    {
        return items.DrillDown<T>(i => i.Descendants());
    }

    public static IEnumerable<T> DescendantsAndSelf<T>(this IEnumerable<AspxNode> items)
        where T : AspxNode
    {
        return items.DrillDown<T>(i => i.DescendantsAndSelf());
    }

    public static IEnumerable<T> Ancestors<T>(this IEnumerable<AspxNode> items)
        where T : AspxNode
    {
        return items.DrillDown<T>(i => i.Ancestors());
    }

    public static IEnumerable<T> AncestorsAndSelf<T>(this IEnumerable<AspxNode> items)
        where T : AspxNode
    {
        return items.DrillDown<T>(i => i.AncestorsAndSelf());
    }

    public static IEnumerable<T> Elements<T>(this IEnumerable<AspxNode> items)
        where T : AspxNode
    {
        return items.DrillDown<T>(i => i.Elements());
    }

    public static IEnumerable<T> ElementsAndSelf<T>(this IEnumerable<AspxNode> items)
        where T : AspxNode
    {
        return items.DrillDown<T>(i => i.ElementsAndSelf());
    }
}
