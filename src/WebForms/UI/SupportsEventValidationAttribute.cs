// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.Collections;

namespace System.Web.UI;
/// <devdoc>
/// <para></para>
/// </devdoc>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class SupportsEventValidationAttribute : Attribute
{
    private static readonly Hashtable _typesSupportsEventValidation = Hashtable.Synchronized(new Hashtable());

    public SupportsEventValidationAttribute()
    {
    }

    internal static bool SupportsEventValidation(Type type)
    {
        object result = _typesSupportsEventValidation[type];
        if (result != null)
        {
            return (bool)result;
        }

        // Check the attributes on the type to see if it supports SupportsEventValidationAttribute
        // Note that this attribute does not inherit from the base class, since derived classes may 
        // not be able to validate properly.
        object[] attribs = type.GetCustomAttributes(typeof(SupportsEventValidationAttribute), false /* inherits */);
        bool supportsEventValidation = (attribs != null) && (attribs.Length > 0);
        _typesSupportsEventValidation[type] = supportsEventValidation;

        return supportsEventValidation;
    }
}

