// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;

namespace System.Web.UI.WebControls;

/* This control represents the ITemplate property on the content page that will be applied
   to the MasterPage template property. The ContentPlaceHolderID is never assigned at runtime. */

[Designer("System.Web.UI.Design.WebControls.ContentDesigner, " + AssemblyRef.SystemDesign)]
[ToolboxItem(false)]
public class Content : Control, INonBindingContainer
{
    private string _contentPlaceHolderID;

    [
    DefaultValue(""),
    IDReferenceProperty(typeof(ContentPlaceHolder)),
    Themeable(false),
    WebCategory("Behavior"),
    WebSysDescription(SR.Content_ContentPlaceHolderID),
    ]
    public string ContentPlaceHolderID
    {
        get
        {
            if (_contentPlaceHolderID == null)
            {
                return String.Empty;
            }
            return _contentPlaceHolderID;
        }
        set
        {
            if (!DesignMode)
            {
                throw new NotSupportedException(SR.GetString(SR.Property_Set_Not_Supported, "ContentPlaceHolderID", this.GetType().ToString()));
            }

            _contentPlaceHolderID = value;
        }
    }

    #region hide these events in the designer since they will not be invoked.
    [
    Browsable(false),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
    ]
    public new event EventHandler DataBinding
    {
        add
        {
            base.DataBinding += value;
        }
        remove
        {
            base.DataBinding -= value;
        }
    }

    [
    Browsable(false),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
    ]
    public new event EventHandler Disposed
    {
        add
        {
            base.Disposed += value;
        }
        remove
        {
            base.Disposed -= value;
        }
    }

    [
    Browsable(false),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
    ]
    public new event EventHandler Init
    {
        add
        {
            base.Init += value;
        }
        remove
        {
            base.Init -= value;
        }
    }

    [
    Browsable(false),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
    ]
    public new event EventHandler Load
    {
        add
        {
            base.Load += value;
        }
        remove
        {
            base.Load -= value;
        }
    }

    [
    Browsable(false),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
    ]
    public new event EventHandler PreRender
    {
        add
        {
            base.PreRender += value;
        }
        remove
        {
            base.PreRender -= value;
        }
    }

    [
    Browsable(false),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
    ]
    public new event EventHandler Unload
    {
        add
        {
            base.Unload += value;
        }
        remove
        {
            base.Unload -= value;
        }
    }
    #endregion
}
