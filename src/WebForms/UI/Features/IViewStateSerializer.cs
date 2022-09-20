// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web.UI.Features;

internal interface IViewStateSerializer
{
    void Serialize(BinaryWriter writer, object obj);

    object? Deserialize(BinaryReader reader);
}
