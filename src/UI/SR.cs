// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web;

internal static class SR
{
    public const string PersonalizationProviderHelper_TrimmedEmptyString = nameof(PersonalizationProviderHelper_TrimmedEmptyString);

    public const string StringUtil_Trimmed_String_Exceed_Maximum_Length = nameof(StringUtil_Trimmed_String_Exceed_Maximum_Length);

    public const string InvalidOffsetOrCount = nameof(InvalidOffsetOrCount);

    public const string Empty_path_has_no_directory = nameof(Empty_path_has_no_directory);

    public const string Path_must_be_rooted = nameof(Path_must_be_rooted);

    public const string HTMLTextWriterUnbalancedPop = nameof(HTMLTextWriterUnbalancedPop);
    public const string StateManagedCollection_NoKnownTypes = nameof(StateManagedCollection_NoKnownTypes);
    public const string StateManagedCollection_InvalidIndex = nameof(StateManagedCollection_InvalidIndex);
    public const string Style_RegisteredStylesAreReadOnly = nameof(Style_RegisteredStylesAreReadOnly);
    public const string Style_BorderColor = nameof(Style_RegisteredStylesAreReadOnly);
    public const string Style_BorderWidth = nameof(Style_RegisteredStylesAreReadOnly);
    public const string Style_BorderStyle = nameof(Style_RegisteredStylesAreReadOnly);
    public const string Style_CSSClass = nameof(Style_RegisteredStylesAreReadOnly);
    public const string Style_Font = nameof(Style_RegisteredStylesAreReadOnly);
    public const string Style_ForeColor = nameof(Style_RegisteredStylesAreReadOnly);
    public const string Style_Height = nameof(Style_RegisteredStylesAreReadOnly);
    public const string Style_Width = nameof(Style_RegisteredStylesAreReadOnly);
    public const string Style_InvalidBorderWidth = nameof(Style_RegisteredStylesAreReadOnly);
    public const string Style_InvalidHeight = nameof(Style_RegisteredStylesAreReadOnly);
    public const string Style_InvalidWidth = nameof(Style_RegisteredStylesAreReadOnly);
    public const string FontInfo_Bold = nameof(Style_RegisteredStylesAreReadOnly);
    public const string FontInfo_Italic = nameof(Style_RegisteredStylesAreReadOnly);
    public const string FontInfo_Name = nameof(Style_RegisteredStylesAreReadOnly);
    public const string FontInfo_Names = nameof(Style_RegisteredStylesAreReadOnly);
    public const string FontInfo_Overline = nameof(Style_RegisteredStylesAreReadOnly);
    public const string FontInfo_Size = nameof(Style_RegisteredStylesAreReadOnly);
    public const string FontInfo_Strikeout = nameof(Style_RegisteredStylesAreReadOnly);
    public const string FontInfo_Underline = nameof(Style_RegisteredStylesAreReadOnly);

    public const string Style_BackColor = nameof(Style_BackColor);

    public static string UnitParseNoDigits = nameof(UnitParseNoDigits);
    public static string UnitParseNumericPart = nameof(UnitParseNumericPart);
    public static string Invalid_app_VirtualPath = nameof(Invalid_app_VirtualPath);
    public static string Physical_path_not_allowed = nameof(Physical_path_not_allowed);
    public static string Invalid_vpath = nameof(Invalid_vpath);

    public static string Type_not_creatable_from_string = nameof(Type_not_creatable_from_string);

    public static string GetString(string name, params object[] args)
        => name;
}
