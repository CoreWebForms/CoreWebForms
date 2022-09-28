// MIT License.

#nullable disable

namespace System.Web.UI;
public interface IResourceUrlGenerator
{
    string GetResourceUrl(Type type, string resourceName);
}
