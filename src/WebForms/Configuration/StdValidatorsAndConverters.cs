// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Web.Configuration;
internal static class StdValidatorsAndConverters
{
    static private TypeConverter s_whiteSpaceTrimStringConverter;
    static private ConfigurationValidatorBase s_nonEmptyStringValidator;

    static internal TypeConverter WhiteSpaceTrimStringConverter
    {
        get
        {
            if (s_whiteSpaceTrimStringConverter == null)
            {
                s_whiteSpaceTrimStringConverter = new WhiteSpaceTrimStringConverter();
            }

            return s_whiteSpaceTrimStringConverter;
        }
    }

    static internal ConfigurationValidatorBase NonEmptyStringValidator
    {
        get
        {
            if (s_nonEmptyStringValidator == null)
            {
                s_nonEmptyStringValidator = new StringValidator(1);
            }

            return s_nonEmptyStringValidator;
        }
    }

}
