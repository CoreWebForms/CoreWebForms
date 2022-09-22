// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Web.UI;
public static class ValidationSettings
{

    private static UnobtrusiveValidationMode? _unobtrusiveValidationMode;

    /// <summary>
    /// Gets or sets the client side validation mode of the application.
    /// </summary>
    public static UnobtrusiveValidationMode UnobtrusiveValidationMode
    {
        get
        {
            if (_unobtrusiveValidationMode == null)
            {

                string configValue = "WebForms"; //ConfigurationManager.AppSettings["ValidationSettings:UnobtrusiveValidationMode"];
                object value = PropertyConverter.EnumFromString(typeof(UnobtrusiveValidationMode), configValue);

                if (value == null)
                {
                    _unobtrusiveValidationMode = UnobtrusiveValidationMode.WebForms;// (BinaryCompatibility.Current.TargetsAtLeastFramework45) ? UnobtrusiveValidationMode.WebForms : UnobtrusiveValidationMode.None;
                }
                else
                {
                    Debug.Assert(value is UnobtrusiveValidationMode);
                    _unobtrusiveValidationMode = (UnobtrusiveValidationMode)value;
                }
            }
            return _unobtrusiveValidationMode.Value;
        }
        set
        {
            if (value < UnobtrusiveValidationMode.None || value > UnobtrusiveValidationMode.WebForms)
            {
                throw new ArgumentOutOfRangeException("value");
            }

            _unobtrusiveValidationMode = value;
        }
    }
}
