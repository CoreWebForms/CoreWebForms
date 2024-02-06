Type._registerScript("MicrosoftAjaxNetwork.js", ["MicrosoftAjaxSerialization.js"]);

import {} from "./src/Sys/XMLHttpRequest.js"

Type.registerNamespace('Sys.Net');

import {} from "./src/Sys/Net/WebRequestExecutor.js"
import {} from "./src/Sys/Net/XMLHttpExecutor.js"
import {} from "./src/Sys/Net/WebRequestManager.js"
import {} from "./src/Sys/Net/NetworkRequestEventArgs.js"
import {} from "./src/Sys/Net/WebRequest.js"

// ScriptLoaderTask required by both WebForms and WebServices (for jsonp support)
// MSAjaxNetwork is a common dependency between them.
import {} from "./src/Sys/ScriptLoaderTask.js"
