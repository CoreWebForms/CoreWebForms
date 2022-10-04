// MIT License.

using System.ComponentModel;

namespace System.Web.UI.WebControls;

[
 Bindable(false),
 ParseChildren(true),
 PersistChildren(false),
 Designer("System.Web.UI.Design.WebControls.LoginViewDesigner," + AssemblyRef.SystemDesign),
 DefaultProperty("CurrentView"),
 DefaultEvent("ViewChanged"),
 Themeable(true),
 ]
public class LoginView : Control, INamingContainer
{
}

[
  Bindable(false),
  DefaultEvent("LoggingOut"),
  Designer("System.Web.UI.Design.WebControls.LoginStatusDesigner, " + AssemblyRef.SystemDesign),
  ]
public class LoginStatus : CompositeControl
{
}

