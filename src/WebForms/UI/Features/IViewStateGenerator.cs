// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Globalization;

namespace System.Web.UI.Features;

internal interface IViewStateManager
{
    string GeneratorId { get; }

    string ClientState { get; }
}

internal class ViewStateManager : IViewStateManager
{
    public ViewStateManager(Type type, HttpContextCore context)
    {
        GeneratorId = type.GetHashCode().ToString("X8", CultureInfo.InvariantCulture);

        if (context.Request.HasFormContentType)
        {
            if (string.Equals(GeneratorId, context.Request.Form[Page.ViewStateGeneratorFieldID], StringComparison.Ordinal))
            {
                ClientState = context.Request.Form[Page.ViewStateFieldPrefixID];
            }
        }
    }

    public string ClientState { get; } = string.Empty;

    public string GeneratorId { get; }
}
