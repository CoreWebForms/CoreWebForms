// MIT License.

using System.Reflection;

namespace System.Web.UI;
internal interface IClientScriptManager
{
    Dictionary<Assembly, Dictionary<String, Object>> RegisteredResourcesToSuppress
    {
        get;
    }
    string GetPostBackEventReference(PostBackOptions options);
    string GetWebResourceUrl(Type type, string resourceName);
    void RegisterClientScriptBlock(Type type, string key, string script);
    void RegisterClientScriptInclude(Type type, string key, string url);
    void RegisterClientScriptBlock(Type type, string key, string script, bool addScriptTags);
    void RegisterStartupScript(Type type, string key, string script, bool addScriptTags);
}
