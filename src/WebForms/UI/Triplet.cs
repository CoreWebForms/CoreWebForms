// MIT License.

#nullable disable

namespace System.Web.UI;
[Serializable]
public sealed class Triplet
{

    public object First;

    public object Second;

    public object Third;

    public Triplet()
    {
    }

    public Triplet(object x, object y)
    {
        First = x;
        Second = y;
    }

    public Triplet(object x, object y, object z)
    {
        First = x;
        Second = y;
        Third = z;
    }
}
