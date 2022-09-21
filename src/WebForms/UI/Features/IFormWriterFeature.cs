// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Web.UI.HtmlControls;

namespace System.Web.UI.Features;

internal interface IFormWriterFeature
{
    void BeginFormRender(HtmlTextWriter writer, string? formUniqueID);
    void OnFormPostRender(HtmlTextWriter writer);
    void OnFormRender();
    void EndFormRender(HtmlTextWriter writer, string formUniqueID);
    void ResetOnFormRenderCalled();
    void AddHiddenField(string name, string value);
    HtmlForm? Form { get; set; }
    bool IsRendering { get; }
}
