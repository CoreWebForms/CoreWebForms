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
    // NOTE: This was done in the Builder on ASP.NET Framework
    protected internal override void CreateChildControls()
    {
        base.CreateChildControls();

        if (Page.Master is { } master)
        {
            if (PageProvidesMatchingContent(master))
            {
                ITemplate tpl = ((System.Web.UI.ITemplate)(master.ContentTemplates[ID]));
                master.InstantiateInContentPlaceHolder(this, tpl);
            }
        }
    }

    private bool PageProvidesMatchingContent(MasterPage masterPage)
    {
        if (masterPage != null && masterPage.ContentTemplates != null
                    && masterPage.ContentTemplates.Contains(ID))
        {
            return true;
        }

        return false;
    }
}
