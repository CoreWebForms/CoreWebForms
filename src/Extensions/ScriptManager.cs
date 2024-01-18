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
    // TODO: use di here instead of static initialization
    [Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1810:Initialize reference type static fields inline", Justification = "Initialize something in a different. Should move to DI")]
    static ScriptManager()
    {
        ClientScriptManager._scriptResourceMapping = new ScriptResourceMapping();
    }

    private static readonly Dictionary<string, string> _knownScripts = new()
    {
        { "MsAjaxBundle", "https://ajax.aspnetcdn.com/ajax/4.5.1/1/MsAjaxBundle.js" },
        { "jquery", "https://ajax.aspnetcdn.com/ajax/jQuery/jquery-3.5.1.min.js" },
        { "bootstrap", "https://ajax.aspnetcdn.com/ajax/bootstrap/4.5.3/bootstrap.min.js" },
        { "respond", "https://cdnjs.cloudflare.com/ajax/libs/respond.js/1.4.0/respond.min.js" },
        { "WebFormsBundle", "https://ajax.aspnetcdn.com/ajax/4.5.1/1/WebFormsBundle.js" },
    };

    private ScriptReferenceCollection _scripts;
    [
    Category("Behavior"),
    Editor("System.Web.UI.Design.CollectionEditorBase, " + AssemblyRef.SystemWebExtensionsDesign, typeof(UITypeEditor)),
    DefaultValue(null),
    PersistenceMode(PersistenceMode.InnerProperty),
    MergableProperty(false),
    ]
    public ScriptReferenceCollection Scripts => _scripts ??= new ScriptReferenceCollection();

    public static ScriptResourceMapping ScriptResourceMapping
    {
        get
        {
            return (ScriptResourceMapping)ClientScriptManager._scriptResourceMapping;
        }
    }

    protected internal override void Render(HtmlTextWriter writer)
    {
        if (_scripts is null)
        {
            return;
        }

        foreach (var script in _scripts)
        {
            if (script.Assembly is { } assembly && script.Path is { })
            {
                writer.Write("<script src=\"__webforms/scripts/");
                writer.Write(assembly);
                writer.Write("/");
                writer.Write(script.Name);
                writer.WriteLine("\" type=\"text/javascript\"></script>");
            }
            else if (_knownScripts.TryGetValue(script.Name, out var knownScript))
            {
                writer.Write("<script src=\"");
                writer.Write(knownScript);
                writer.WriteLine("\" type=\"text/javascript\"></script>");
            }
            else
            {
                writer.Write("<!-- Unknown script '");
                writer.Write(script.Name);
                writer.WriteLine("' -->");
            }
        }
    }
}
