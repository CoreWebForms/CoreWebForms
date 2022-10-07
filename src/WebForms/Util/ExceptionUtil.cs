// MIT License.

namespace System.Web.Util;

using System;
using System.Web;

internal static class ExceptionUtil
{
    internal static ArgumentException ParameterInvalid(string parameter)
    {
        return new ArgumentException(SR.GetString(SR.Parameter_Invalid, parameter), parameter);
    }

    internal static ArgumentException ParameterNullOrEmpty(string parameter)
    {
        return new ArgumentException(SR.GetString(SR.Parameter_NullOrEmpty, parameter), parameter);
    }

    internal static ArgumentException PropertyInvalid(string property)
    {
        return new ArgumentException(SR.GetString(SR.Property_Invalid, property), property);
    }

    internal static ArgumentException PropertyNullOrEmpty(string property)
    {
        return new ArgumentException(SR.GetString(SR.Property_NullOrEmpty, property), property);
    }

    internal static InvalidOperationException UnexpectedError(string methodName)
    {
        return new InvalidOperationException(SR.GetString(SR.Unexpected_Error, methodName));
    }
}

