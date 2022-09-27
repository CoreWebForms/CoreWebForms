// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web.ModelBinding;

/// <summary>
/// This interface provides a way for model binding system to use custom value providers like
/// Form, QueryString, ViewState.
/// </summary>
public interface IValueProviderSource
{
    IValueProvider GetValueProvider(ModelBindingExecutionContext modelBindingExecutionContext);
}
