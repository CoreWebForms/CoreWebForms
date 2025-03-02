// MIT License.
using System.Diagnostics.CodeAnalysis;

namespace System.Web.Script.Serialization;
public abstract class JavaScriptConverter
{
    public abstract IEnumerable<Type> SupportedTypes
    {
        get;
    }

    public abstract object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer);

    [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "obj",
        Justification = Constant.CA1720)]
    public abstract IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer);
}
