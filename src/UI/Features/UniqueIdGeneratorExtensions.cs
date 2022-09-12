// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web.UI.Features;

internal static class UniqueIdGeneratorExtensions
{
    public static void EnableUniqueIdGenerator(this Control control)
    {
        control.Features.Set<IUniqueIdGeneratorFeature>(new UniqueIdGeneratorFeature(control));
    }
}
