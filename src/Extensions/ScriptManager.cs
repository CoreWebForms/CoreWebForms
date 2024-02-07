// MIT License.

using System.ComponentModel;
using System.Diagnostics;
using System.Web.Resources;

namespace System.Web.UI;

[
DefaultProperty("Scripts"),
Designer("System.Web.UI.Design.ScriptManagerDesigner, " + AssemblyRef.SystemWebExtensionsDesign),
NonVisualControl(),
ParseChildren(true),
PersistChildren(false),
]
[Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Still working on implementing")]
[Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Still working on implementing")]
public class ScriptManager : Control, IScriptManagerInternal, IScriptManager
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

    public bool EnablePartialRendering { get; set; }

    internal bool SupportsPartialRendering { get; set; }

    string IScriptManagerInternal.AsyncPostBackSourceElementID
    {
        get
        {
            Debugger.Break();
            return "";
        }
    }

    bool IScriptManagerInternal.SupportsPartialRendering => true;

    bool IScriptManagerInternal.IsInAsyncPostBack => false;

    bool IScriptManager.SupportsPartialRendering
    {
        get
        {
            Debugger.Break();
            return false;
        }
    }

    bool IScriptManager.IsInAsyncPostBack
    {
        get
        {
            Debugger.Break();
            return false;
        }
    }

    bool IScriptManager.EnableCdn
    {
        get
        {
            Debugger.Break();
            return false;
        }
    }

    bool IScriptManager.EnableCdnFallback
    {
        get
        {
            Debugger.Break();
            return false;
        }
    }

    bool IScriptManager.IsDebuggingEnabled
    {
        get
        {
            Debugger.Break();
            return false;
        }
    }

    bool IScriptManager.IsSecureConnection
    {
        get
        {
            Debugger.Break();
            return false;
        }
    }

    protected internal override void OnInit(EventArgs e)
    {
        base.OnInit(e);

        if (!DesignMode)
        {
            ScriptManager existingInstance = ScriptManager.GetCurrent(Page);

            if (existingInstance != null)
            {
                throw new InvalidOperationException(AtlasWeb.ScriptManager_OnlyOneScriptManager);
            }

            var page = Page;

            page.Items[typeof(IScriptManager)] = this;
            page.Items[typeof(ScriptManager)] = this;

#if FALSE
            page.InitComplete += OnPageInitComplete;
            page.PreRenderComplete += OnPagePreRenderComplete;

            if (page.IsPostBack)
            {
                _isInAsyncPostBack = PageRequestManager.IsAsyncPostBackRequest(page.Request);
            }
            // Delegate to PageRequestManager to hook up error handling for async posts
            PageRequestManager.OnInit();

            page.PreRender += ScriptControlManager.OnPagePreRender;
#endif
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

    internal void RegisterDispose(Control owner, string v) =>
        // TODO
        Debugger.Break();

    internal static ScriptManager GetCurrent(Page page) => page.Items[typeof(ScriptManager)] as ScriptManager;

    internal void RegisterScriptDescriptors(IExtenderControl descriptor) => Debugger.Break();

    internal void RegisterScriptDescriptors(IScriptControl descriptor) => Debugger.Break();

    internal void RegisterScriptControl<TScriptControl>(TScriptControl scriptControl)
        where TScriptControl : Control, IScriptControl => Debugger.Break();

    void IScriptManagerInternal.RegisterAsyncPostBackControl(Control control) => Debugger.Break();

    void IScriptManagerInternal.RegisterExtenderControl<TExtenderControl>(TExtenderControl extenderControl, Control targetControl) => Debugger.Break();

    void IScriptManagerInternal.RegisterPostBackControl(Control control) => Debugger.Break();

    void IScriptManagerInternal.RegisterScriptControl<TScriptControl>(TScriptControl scriptControl) => Debugger.Break();

    void IScriptManagerInternal.RegisterScriptDescriptors(IExtenderControl extenderControl) => Debugger.Break();

    void IScriptManagerInternal.RegisterScriptDescriptors(IScriptControl scriptControl) => Debugger.Break();

    void IScriptManagerInternal.RegisterUpdatePanel(UpdatePanel updatePanel) => Debugger.Break();

    void IScriptManagerInternal.UnregisterUpdatePanel(UpdatePanel updatePanel) => Debugger.Break();

    void IScriptManager.RegisterArrayDeclaration(Control control, string arrayName, string arrayValue) => Debugger.Break();

    void IScriptManager.RegisterClientScriptBlock(Control control, Type type, string key, string script, bool addScriptTags) => Debugger.Break();

    void IScriptManager.RegisterClientScriptInclude(Control control, Type type, string key, string url) => Debugger.Break();

    void IScriptManager.RegisterClientScriptResource(Control control, Type type, string resourceName) => Debugger.Break();

    void IScriptManager.RegisterDispose(Control control, string disposeScript) => Debugger.Break();

    void IScriptManager.RegisterExpandoAttribute(Control control, string controlId, string attributeName, string attributeValue, bool encode) => Debugger.Break();

    void IScriptManager.RegisterHiddenField(Control control, string hiddenFieldName, string hiddenFieldValue) => Debugger.Break();

    void IScriptManager.RegisterOnSubmitStatement(Control control, Type type, string key, string script) => Debugger.Break();

    void IScriptManager.RegisterPostBackControl(Control control) => Debugger.Break();

    void IScriptManager.RegisterStartupScript(Control control, Type type, string key, string script, bool addScriptTags) => Debugger.Break();

    void IScriptManager.SetFocusInternal(string clientID) => Debugger.Break();
}
