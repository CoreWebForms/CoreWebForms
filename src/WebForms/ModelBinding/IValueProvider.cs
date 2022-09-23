// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web.ModelBinding {

    public interface IValueProvider {
        bool ContainsPrefix(string prefix);
        ValueProviderResult GetValue(string key);
    }
}
