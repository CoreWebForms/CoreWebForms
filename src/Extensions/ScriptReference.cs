// MIT License.

using System.ComponentModel;

namespace System.Web.UI;

[
  DefaultProperty("Path"),
  ]
public class ScriptReference : ScriptReferenceBase
{

    [
    Category("Behavior"),
    DefaultValue(""),
    ]
    public string Name { get; set; }

    [
    Category("Behavior"),
    DefaultValue(""),
    ]
    public string Assembly { get; set; }
}
