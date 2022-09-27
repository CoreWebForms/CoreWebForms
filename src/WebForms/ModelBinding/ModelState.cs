// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web.ModelBinding;

using System;

[Serializable]
public class ModelState
{

    private readonly ModelErrorCollection _errors = new ModelErrorCollection();

    public ValueProviderResult Value
    {
        get;
        set;
    }

    public ModelErrorCollection Errors
    {
        get
        {
            return _errors;
        }
    }
}
