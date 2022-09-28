// MIT License.

#nullable disable

namespace System.Web.UI;
[Serializable]
public sealed class Pair
{

    public object First;

    public object Second;

    public Pair()
    {
    }

    public Pair(object x, object y)
    {
        First = x;
        Second = y;
    }
}
