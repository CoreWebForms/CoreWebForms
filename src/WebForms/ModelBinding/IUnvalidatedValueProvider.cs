// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web.ModelBinding;

// Represents a special IValueProvider that has the ability to skip request validation.
public interface IUnvalidatedValueProvider : IValueProvider
{
    ValueProviderResult GetValue(string key, bool skipValidation);
}
