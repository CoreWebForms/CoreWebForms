// MIT License.

using System.ComponentModel;

namespace System.Web.UI;

[
DefaultProperty("Scripts"),
Designer("System.Web.UI.Design.ScriptManagerDesigner, " + AssemblyRef.SystemWebExtensionsDesign),
NonVisualControl(),
ParseChildren(true),
PersistChildren(false),
]
public class ScriptManager : Control
{
    private ScriptReferenceCollection _scripts;
    [
    Category("Behavior"),
    Editor("System.Web.UI.Design.CollectionEditorBase, " + AssemblyRef.SystemWebExtensionsDesign, typeof(UITypeEditor)),
    DefaultValue(null),
    PersistenceMode(PersistenceMode.InnerProperty),
    MergableProperty(false),
    ]
    public ScriptReferenceCollection Scripts => _scripts ??= new ScriptReferenceCollection();
}
