// MIT License.

using System.ComponentModel;

namespace System.Web.UI;

public class ScriptReferenceBase
{
    [
     Category("Behavior"),
     DefaultValue(""),
     NotifyParentProperty(true),
     UrlProperty("*.js")
     ]
    public string Path { get; set; }
}
