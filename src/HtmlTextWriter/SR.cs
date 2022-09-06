using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Web;

internal static class SR
{
    public static string PersonalizationProviderHelper_TrimmedEmptyString => nameof(PersonalizationProviderHelper_TrimmedEmptyString);

    public static string StringUtil_Trimmed_String_Exceed_Maximum_Length => nameof(StringUtil_Trimmed_String_Exceed_Maximum_Length);

    public static string InvalidOffsetOrCount => nameof(InvalidOffsetOrCount);

    public static string Empty_path_has_no_directory => nameof(Empty_path_has_no_directory);

    public static string Path_must_be_rooted => nameof(Path_must_be_rooted);

    public static string HTMLTextWriterUnbalancedPop { get; internal set; }
    public static string StateManagedCollection_NoKnownTypes { get; internal set; }
    public static string StateManagedCollection_InvalidIndex { get; internal set; }

    public const string Style_BackColor = nameof(Style_BackColor);

    public static string GetString(string name, params object[] args)
        => name;
}
