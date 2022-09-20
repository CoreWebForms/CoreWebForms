// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using System.Web.UI.HtmlControls;

namespace System.Web.UI.Features;

#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable CA1823 // Avoid unused private fields
#pragma warning disable CA1822 // Mark members as static
#pragma warning disable IDE0044 // Add readonly modifier

internal class FormWriterFeature : IFormWriterFeature
{
    private const string HiddenClassName = "aspNetHidden";

    public FormWriterFeature(Page owner, ClientScriptManager clientScript)
    {
        _owner = owner;
        ClientScript = clientScript;
    }

    private readonly Page _owner;

    public ClientScriptManager ClientScript { get; }

    private bool _fOnFormRenderCalled;
    private bool _fRequireWebFormsScript;
    private bool _fWebFormsScriptRendered;
    private bool _fRequirePostBackScript;
    private bool _fPostBackScriptRendered;
    private bool _containsCrossPagePost;
    private Dictionary<String, String>? _hiddenFieldsToRender;

    public bool ClientSupportsJavaScript => true;

    public void OnFormRender()
    {
        if (_fOnFormRenderCalled)
        {
            throw new HttpException(SR.GetString("Multiple_forms_not_allowed"));
        }

        _fOnFormRenderCalled = true;
        IsRendering = true;
    }

    public HtmlForm? Form { get; set; }

    public bool IsRendering { get; private set; }

    public void BeginFormRender(HtmlTextWriter writer, string? formUniqueID)
    {
        writer.AddAttribute("class", HiddenClassName);
        writer.RenderBeginTag("div");
        writer.RenderEndTag();

        ClientScript.RenderHiddenFields(writer);
        RenderViewStateFields(writer);
    }

    private void RenderViewStateFields(HtmlTextWriter writer)
    {
        if (_hiddenFieldsToRender == null)
        {
            _hiddenFieldsToRender = new Dictionary<string, string>();
        }
        if (_owner.ClientState != null)
        {
            var viewStateChunks = DecomposeViewStateIntoChunksInternal();

            writer.WriteLine();

            // Don't write out a view state field count if there is only 1 viewstate field
            if (viewStateChunks.Count > 1)
            {
                string value = viewStateChunks.Count.ToString(CultureInfo.InvariantCulture);
                writer.Write("<input type=\"hidden\" name=\"");
                writer.Write(Page.ViewStateFieldCountID);
                writer.Write("\" id=\"");
                writer.Write(Page.ViewStateFieldCountID);
                writer.Write("\" value=\"");
                writer.Write(value);
                writer.WriteLine("\" />");
                _hiddenFieldsToRender[Page.ViewStateFieldCountID] = value;
            }

            int count = 0;
            foreach (string stateChunk in viewStateChunks)
            {
                writer.Write("<input type=\"hidden\" name=\"");
                string name = Page.ViewStateFieldPrefixID;
                writer.Write(Page.ViewStateFieldPrefixID);
                string? countString = null;
                if (count > 0)
                {
                    countString = count.ToString(CultureInfo.InvariantCulture);
                    name += countString;
                    writer.Write(countString);
                }
                writer.Write("\" id=\"");
                writer.Write(name);
                writer.Write("\" value=\"");
                writer.Write(stateChunk);
                writer.WriteLine("\" />");
                ++count;
                _hiddenFieldsToRender[name] = stateChunk;
            }

            // DevDiv #461378: Write out an identifier so we know who generated this __VIEWSTATE field.
            // It doesn't need to be MACed since the only thing we use it for is error suppression,
            // similar to how __PREVIOUSPAGE works.
            ClientScript.RegisterHiddenField(Page.ViewStateGeneratorFieldID, _owner.Features.GetRequired<IViewStateManager>().GeneratorId);
        }
        else
        {
            // ASURT 106992
            // Need to always render out the viewstate field so alternate viewstate persistence will get called
            writer.Write("\r\n<input type=\"hidden\" name=\"");
            writer.Write(Page.ViewStateFieldPrefixID);
            // Dev10 Bug 486494
            // Remove previously rendered NewLine
            writer.Write("\" id=\"");
            writer.Write(Page.ViewStateFieldPrefixID);
            writer.WriteLine("\" value=\"\" />");
            _hiddenFieldsToRender[Page.ViewStateFieldPrefixID] = String.Empty;
        }
    }

    /// <devdoc>
    ///     Called by both adapters and default rendering after form rendering.
    /// </devdoc>
    public void OnFormPostRender(HtmlTextWriter writer)
    {
        IsRendering = false;
        //if (_postFormRenderDelegate != null)
        //{
        //    _postFormRenderDelegate(writer, null);
        //}
    }

    public void ResetOnFormRenderCalled()
    {
        _fOnFormRenderCalled = false;
    }

    public void EndFormRender(HtmlTextWriter writer, string formUniqueID)
    {
        EndFormRenderArrayAndExpandoAttribute(writer, formUniqueID);
        EndFormRenderHiddenFields(writer, formUniqueID);
        EndFormRenderPostBackAndWebFormsScript(writer, formUniqueID);
    }

    private void EndFormRenderArrayAndExpandoAttribute(HtmlTextWriter writer, string formUniqueID)
    {
        if (ClientSupportsJavaScript)
        {
#if FALSE
            // Devdiv 9409 - Register the array for reenabling only after the controls have been processed,
            // so that list controls can have their children registered.
            if (_owner.RenderDisabledControlsScript)
            {
                foreach (Control control in _owner.EnabledControls)
                {
                    ClientScript.RegisterArrayDeclaration(EnabledControlArray, "'" + control.ClientID + "'");
                }
            }
#endif
            ClientScript.RenderArrayDeclares(writer);
            ClientScript.RenderExpandoAttribute(writer);
        }
    }

    public bool RequiresViewStateEncryptionInternal => false;

    internal void EndFormRenderHiddenFields(HtmlTextWriter writer, string formUniqueID)
    {
        if (RequiresViewStateEncryptionInternal)
        {
            ClientScript.RegisterHiddenField(Page.ViewStateEncryptionID, String.Empty);
        }

#if FALSE
        if (_containsCrossPagePost)
        {
            string path = EncryptString(Request.CurrentExecutionFilePath, Purpose.WebForms_Page_PreviousPageID);
            ClientScript.RegisterHiddenField(previousPageID, path);
        }
#endif

        if (_owner.EnableEventValidation)
        {
            ClientScript.SaveEventValidationField();
        }

        if (ClientScript.HasRegisteredHiddenFields)
        {
            writer.WriteLine();
            writer.AddAttribute(HtmlTextWriterAttribute.Class, HiddenClassName);
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            ClientScript.RenderHiddenFields(writer);

            writer.RenderEndTag();
        }
    }

    private void EndFormRenderPostBackAndWebFormsScript(HtmlTextWriter writer, string formUniqueID)
    {
#if FALSE
        if (ClientSupportsJavaScript)
        {
            if (_fRequirePostBackScript && !_fPostBackScriptRendered)
            {
                RenderPostBackScript(writer, formUniqueID);
            }

            if (_fRequireWebFormsScript && !_fWebFormsScriptRendered)
                RenderWebFormsScript(writer);
        }

        ClientScript.RenderClientStartupScripts(writer);
#endif
    }

    internal IReadOnlyCollection<string> DecomposeViewStateIntoChunksInternal()
    {
        string state = _owner.ClientState;
        if (state == null)
        {
            return Array.Empty<string>();
        }

        var MaxPageStateFieldLength = _owner.MaxPageStateFieldLength;

        // Any value less than or equal to 0 turns off chunking
        if (MaxPageStateFieldLength <= 0)
        {
            return new[] { state };
        }

        // Break up the view state into the correctly sized chunks
        int numFullChunks = state.Length / _owner.MaxPageStateFieldLength;
        var viewStateChunks = new List<string>(numFullChunks + 1);
        int curPos = 0;
        for (int i = 0; i < numFullChunks; i++)
        {
            viewStateChunks.Add(state.Substring(curPos, MaxPageStateFieldLength));
            curPos += MaxPageStateFieldLength;
        }
        // Add the leftover characters
        if (curPos < state.Length)
        {
            viewStateChunks.Add(state.Substring(curPos));
        }

        // Always want to return at least one empty chunk
        if (viewStateChunks.Count == 0)
        {
            viewStateChunks.Add(string.Empty);
        }
        return viewStateChunks;
    }

    public void AddHiddenField(string name, string value)
        => (_hiddenFieldsToRender ??= new())[name] = value;
}
