// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Web.Compilation;
using System.Web.UI.Features;
using System.Web.UI.HtmlControls;
using System.Web.Util;

// TODO: Remove once implemented
#pragma warning disable CA1822 // Mark members as static
#pragma warning disable CA1823 // Avoid unused private fields

namespace System.Web.UI;

public class Page : TemplateControl, IHttpAsyncHandler
{
    internal const string systemPostFieldPrefix = "__";

    /// <internalonly/>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal const string postEventSourceID = systemPostFieldPrefix + "EVENTTARGET";


    private string _focusedControlID;
    private Control _focusedControl;
    private string _validatorInvalidControl;

    private const string lastFocusID = systemPostFieldPrefix + "LASTFOCUS";
    private const string _scrollPositionXID = systemPostFieldPrefix + "SCROLLPOSITIONX";
    private const string _scrollPositionYID = systemPostFieldPrefix + "SCROLLPOSITIONY";

    /// <internalonly/>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal const string postEventArgumentID = systemPostFieldPrefix + "EVENTARGUMENT";

    internal const string ViewStateFieldPrefixID = systemPostFieldPrefix + "VIEWSTATE";
    internal const string ViewStateFieldCountID = ViewStateFieldPrefixID + "FIELDCOUNT";
    internal const string ViewStateGeneratorFieldID = ViewStateFieldPrefixID + "GENERATOR";
    internal const string ViewStateEncryptionID = systemPostFieldPrefix + "VIEWSTATEENCRYPTED";
    internal const string EventValidationPrefixID = systemPostFieldPrefix + "EVENTVALIDATION";

    internal static readonly object EventInitComplete = new();
    internal static readonly object EventPreLoad = new();
    internal static readonly object EventPreInit = new();
    internal static readonly object EventLoadComplete = new();
    internal static readonly object EventPreRenderComplete = new();
    internal static readonly object EventSaveStateComplete = new();

    private ClientScriptManager? _clientScriptManager;
    // Needed to support Validators in AJAX 1.0 (Windows OS Bugs 2015831)
    private static Type _scriptManagerType;

    public Page()
    {
    }

    bool IHttpHandler.IsReusable => false;

    IAsyncResult IHttpAsyncHandler.BeginProcessRequest(HttpContext context, AsyncCallback cb, object? extraData)
        => TaskAsyncHelper.BeginTask(() => ProcessAsync(context), cb, extraData);

    void IHttpAsyncHandler.EndProcessRequest(IAsyncResult result)
        => TaskAsyncHelper.EndTask(result);

    void IHttpHandler.ProcessRequest(HttpContext context)
        => throw new InvalidOperationException();

    internal bool EnableEventValidation => false;

    public bool IsCallback { get; private set; }

    internal bool IsInOnFormRender => Features.GetRequired<IFormWriterFeature>().IsRendering;

    internal bool ContainsCrossPagePost { get; set; }

    internal Task ProcessAsync(HttpContext context)
    {
        context.Response.ContentType = "text/html";

        if (Features.Get<HttpContext>() is { })
        {
            throw new InvalidOperationException("Page has already been processed.");
        }

        var events = Features.Get<IPageEvents>()!;

        events.OnPreInit();

        InitializeComponents();

        if (Features.Get<IViewStateManager>() is { } viewState)
        {
            viewState.RefreshControls();
        }

        Features.Set(context);

        events.OnPageLoad();

        using var writer = new HtmlTextWriter(context.Response.Output);

        Render(writer);

        return Task.CompletedTask;
    }

    public HtmlForm? Form => Features.Get<IFormWriterFeature>()?.Form;

    public ClientScriptManager ClientScript => _clientScriptManager ??= new ClientScriptManager(this);

    internal IScriptManager ScriptManager => Features.GetRequired<IScriptManager>();

    public bool IsPostBackEventControlRegistered { get; internal set; }

    internal Control? AutoPostBackControl { get; set; }

    protected virtual void InitializeComponents()
    {
    }

    private List<Control>? _enabledControls;

    private List<Control> EnabledControls => _enabledControls ??= new();

    internal void RegisterEnabledControl(Control control)
        => EnabledControls.Add(control);

    internal void RegisterWebFormsScript()
    {
    }

    internal void RegisterPostBackScript()
    {
        ClientScript.RegisterHiddenField(postEventSourceID, string.Empty);
        ClientScript.RegisterHiddenField(postEventArgumentID, string.Empty);
    }

    internal void RegisterFocusScript()
    {
    }

    internal void Validate(string validationGroup)
    {
    }

    internal NameValueCollection RequestValueCollection => Context.Request.Params;

    internal IStateFormatter2 CreateStateFormatter() => Features.GetRequired<IStateFormatter2>();

    internal bool ShouldSuppressMacValidationException(Exception ex) => true;

    internal void VerifyRenderingInServerForm(Control control)
    {
        if (!Features.GetRequired<IFormWriterFeature>().IsRendering)
        {
            throw new InvalidOperationException("Must be in a form");
        }
    }

    internal IReadOnlyCollection<object> GetValidators(string validationGroup)
        => Array.Empty<object>();

    internal bool GetDesignModeInternal() => false;

    internal void PushDataBindingContext(object dataItem)
    {
    }

    internal void PopDataBindingContext()
    {
        throw new NotImplementedException();
    }

    internal void ApplyControlSkin(Control control)
    {
        throw new NotImplementedException();
    }

    internal bool ApplyControlStyleSheet(Control control)
    {
        throw new NotImplementedException();
    }

    internal Task GetWaitForPreviousStepCompletionAwaitable() => Task.CompletedTask;

    internal void RegisterRequiresClearChildControlState(Control control)
    {
        throw new NotImplementedException();
    }

    internal void SetFocus(Control control)
    {
        throw new NotImplementedException();
    }

    internal bool ShouldLoadControlState(Control control)
    {
        throw new NotImplementedException();
    }

    internal bool RequiresControlState(Control control)
    {
        throw new NotImplementedException();
    }

    internal void UnregisterRequiresControlState(Control control)
    {
        throw new NotImplementedException();
    }

    internal void RegisterRequiresControlState(Control control)
    {
        throw new NotImplementedException();
    }

    internal void EndFormRender(HtmlTextWriter writer, string uniqueID)
        => Features.GetRequired<IFormWriterFeature>().EndFormRender(writer, uniqueID);

    internal void OnFormPostRender(HtmlTextWriter writer)
        => Features.GetRequired<IFormWriterFeature>().OnFormPostRender(writer);

    internal void OnFormRender()
        => Features.GetRequired<IFormWriterFeature>().OnFormRender();

    internal void BeginFormRender(HtmlTextWriter writer, string uniqueID)
        => Features.GetRequired<IFormWriterFeature>().BeginFormRender(writer, uniqueID);

    internal void SetForm(HtmlForm htmlForm)
        => Features.GetRequired<IFormWriterFeature>().Form = htmlForm;

    internal void RegisterViewStateHandler()
    {
        Features.GetRequired<IViewStateManager>().UpdateClientState();
    }

    internal bool ClientSupportsJavaScript => true;

    internal bool SupportsCallback => true;

    internal HttpRequest? RequestInternal => Context.Request;

    public string? ClientOnSubmitEvent { get; internal set; }

    internal string ClientState => Features.GetRequired<IViewStateManager>().ClientState;

    internal string RequestViewStateString => Features.GetRequired<IViewStateManager>().OriginalState;

    public bool RenderDisabledControlsScript { get; internal set; }

    public int MaxPageStateFieldLength { get; internal set; } = 1000;
    public bool ContainsTheme { get; internal set; }
    public bool IsPostBack { get; internal set; }

    // const masks into the BitVector32
    private const int styleSheetInitialized = 0x00000001;
    private const int isExportingWebPart = 0x00000002;
    private const int isExportingWebPartShared = 0x00000004;
    private const int isCrossPagePostRequest = 0x00000008;
    // Needed to support Validators in AJAX 1.0 (Windows OS Bugs 2015831)
    private const int isPartialRenderingSupported = 0x00000010;
    private const int isPartialRenderingSupportedSet = 0x00000020;
    private const int skipFormActionValidation = 0x00000040;
    private const int wasViewStateMacErrorSuppressed = 0x00000080;

    // Todo: Move boolean fields into _pageFlags.
#pragma warning disable 0649
    private SimpleBitVector32 _pageFlags;
#pragma warning restore 0649

    // Needed to support Validators in AJAX 1.0 (Windows OS Bugs 2015831)
    #region Atlas ScriptManager Partial Rendering support
    internal bool IsPartialRenderingSupported
    {
        get
        {
            if (!_pageFlags[isPartialRenderingSupportedSet])
            {
                Type scriptManagerType = ScriptManagerType;
                if (scriptManagerType != null)
                {
                    object scriptManager = Page.Items[scriptManagerType];
                    if (scriptManager != null)
                    {
                        PropertyInfo supportsPartialRenderingProperty = scriptManagerType.GetProperty("SupportsPartialRendering");
                        if (supportsPartialRenderingProperty != null)
                        {
                            object supportsPartialRenderingValue = supportsPartialRenderingProperty.GetValue(scriptManager, null);
                            _pageFlags[isPartialRenderingSupported] = (bool)supportsPartialRenderingValue;
                        }
                    }
                }
                _pageFlags[isPartialRenderingSupportedSet] = true;
            }
            return _pageFlags[isPartialRenderingSupported];
        }
    }

    internal Type ScriptManagerType
    {
        get
        {
            if (_scriptManagerType == null)
            {
                _scriptManagerType = BuildManager.GetType("System.Web.UI.ScriptManager", false);
            }
            return _scriptManagerType;
        }
        set
        {
            // Meant for unit testing
            _scriptManagerType = value;
        }
    }

    // For use by controls to store information with same lifetime as the request, for example, all radio buttons can
    // use this to store the dictionary of radio button groups.  The key should be a type.  For the example, the value associated with
    // the key System.Web.UI.WebControls.WmlRadioButtonAdapter is a NameValueCollection of RadioButtonGroups.
    private System.Collections.IDictionary _items;

    [
    Browsable(false)
    ]
    public System.Collections.IDictionary Items
    {
        get
        {
            if (_items == null)
            {
                _items = new HybridDictionary();
            }
            return _items;
        }
    }
    #endregion

    private UnobtrusiveValidationMode? _unobtrusiveValidationMode;
    public UnobtrusiveValidationMode UnobtrusiveValidationMode
    {
        get
        {
            return _unobtrusiveValidationMode ?? ValidationSettings.UnobtrusiveValidationMode;
        }
        set
        {
            if (value < UnobtrusiveValidationMode.None || value > UnobtrusiveValidationMode.WebForms)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            _unobtrusiveValidationMode = value;
        }
    }

    public HttpRequest Request
    {
        get
        {
            if (Context?.Request == null)
                throw new HttpException(SR.GetString(SR.Request_not_available));

            return Context.Request;
        }
    }

    private ValidatorCollection _validators;
    private bool _validated;

    public ValidatorCollection Validators
    {
        get
        {
            if (_validators == null)
            {
                _validators = new ValidatorCollection();
            }
            return _validators;
        }
    }

    public bool IsValid
    {
        get
        {
            if (!_validated)
                throw new HttpException(SR.GetString(SR.IsValid_Cant_Be_Called));

            if (_validators != null)
            {
                ValidatorCollection vc = Validators;
                int count = vc.Count;
                for (int i = 0; i < count; i++)
                {
                    if (!vc[i].IsValid)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }

    internal void SetValidatorInvalidControlFocus(string clientID)
    {
        if (String.IsNullOrEmpty(_validatorInvalidControl))
        {
            _validatorInvalidControl = clientID;

            RegisterFocusScript();
        }
    }
}
