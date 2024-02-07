// Support for partial Rendering

Type._registerScript("MicrosoftAjaxWebForms.js", [
  "MicrosoftAjaxCore.js",
  "MicrosoftAjaxSerialization.js",
  "MicrosoftAjaxNetwork.js",
  "MicrosoftAjaxComponentModel.js",
]);

Type.registerNamespace("Sys.WebForms");

import {} from "./src/Sys/WebForms/BeginRequestEventArgs.js";
import {} from "./src/Sys/WebForms/EndRequestEventArgs.js";
import {} from "./src/Sys/WebForms/InitializeRequestEventArgs.js";
import {} from "./src/Sys/WebForms/PageLoadedEventArgs.js";
import {} from "./src/Sys/WebForms/PageLoadingEventArgs.js";
import {} from "./src/Sys/ScriptLoader.js";
import {} from "./src/Sys/WebForms/PageRequestManager.js";
import {} from "./src/Sys/UI/Controls/UpdateProgress.js";
