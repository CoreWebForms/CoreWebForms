// MIT License.

using System.CodeDom;

namespace System.Web.Profile;

internal class ProfileNameTypeStruct {
    internal string Name;
    internal CodeTypeReference PropertyCodeRefType;
    internal Type PropertyType;
    internal bool IsReadOnly;
    internal int LineNumber;
    internal string FileName;
}
