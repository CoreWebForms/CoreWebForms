// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;

namespace System.Web.UI.WebControls;

[Designer("System.Web.UI.Design.WebControls.ContentPlaceHolderDesigner, " + AssemblyRef.SystemDesign)]
[ToolboxItemFilter("System.Web.UI")]
[ToolboxItemFilter("Microsoft.VisualStudio.Web.WebForms.MasterPageWebFormDesigner", ToolboxItemFilterType.Require)]
[ToolboxData("<{0}:ContentPlaceHolder runat=\"server\"></{0}:ContentPlaceHolder>")]
public class ContentPlaceHolder : Control, INonBindingContainer
{
}
