// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web.ModelBinding;

/// <summary>
/// This represents an IValueProviderSource that supports skipping request validation.
/// </summary>
public interface IUnvalidatedValueProviderSource : IValueProviderSource
{
    bool ValidateInput
    {
        get;
        set;
    }
}
