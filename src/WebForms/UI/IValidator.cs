// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web.UI;

public interface IValidator
{

    bool IsValid
    {
        get;
        set;
    }

    string ErrorMessage
    {
        get;
        set;
    }

    void Validate();
}
