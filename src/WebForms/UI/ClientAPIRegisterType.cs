// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
