// MIT License.

namespace System.Web.Script.Serialization;
public abstract class JavaScriptTypeResolver
{
    public abstract Type ResolveType(string id);
    public abstract string ResolveTypeId(Type type);
}
