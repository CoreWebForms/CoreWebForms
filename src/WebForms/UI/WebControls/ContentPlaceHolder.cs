// MIT License.

using System.ComponentModel;

namespace System.Web.UI.WebControls;

[Designer("System.Web.UI.Design.WebControls.ContentPlaceHolderDesigner, " + AssemblyRef.SystemDesign)]
[ToolboxItemFilter("System.Web.UI")]
[ToolboxItemFilter("Microsoft.VisualStudio.Web.WebForms.MasterPageWebFormDesigner", ToolboxItemFilterType.Require)]
[ToolboxData("<{0}:ContentPlaceHolder runat=\"server\"></{0}:ContentPlaceHolder>")]
public class ContentPlaceHolder : Control, INonBindingContainer
{
}
