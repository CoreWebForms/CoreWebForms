// MIT License.

#nullable disable

namespace System.Web.UI;

// The various types of client API's that can be registered
internal enum ClientAPIRegisterType
{
    WebFormsScript,
    PostBackScript,
    FocusScript,
    ClientScriptBlocks,
    ClientScriptBlocksWithoutTags,
    ClientStartupScripts,
    ClientStartupScriptsWithoutTags,
    OnSubmitStatement,
    ArrayDeclaration,
    HiddenField,
    ExpandoAttribute,
    EventValidation,
}
