// MIT License.

namespace System.Web.UI;
[Serializable]
public sealed class IndexedString
{
    public IndexedString(string s)
    {
        if (string.IsNullOrEmpty(s))
        {
            throw new ArgumentNullException(nameof(s));
        }

        Value = s;
    }

    public string Value { get; }
}
